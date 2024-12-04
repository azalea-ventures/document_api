using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.Storage.Blobs;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDocumentClassificationProvider, DocumentClassificationProvider>();

string? blobConnectionString =
    Environment.GetEnvironmentVariable("REVISA_BUCKET")
    ?? builder.Configuration.GetConnectionString("REVISA_BUCKET");
builder.Services.AddSingleton(x => new BlobServiceClient(blobConnectionString));

builder.Services.AddScoped<IPdfSplitter, PdfSplitter>();
builder.Services.AddScoped<ITextExtractionProvider, TextExtractionProvider>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost(
        "/classify",
        async Task<List<Document>> (
            string blobName,
            string pageRange,
            IDocumentClassificationProvider provider
        ) =>
        {
            var result = await provider.ClassifyDocumentAsync(blobName, pageRange);

            return result
                .Select(doc => new Document
                {
                    DocType = doc.DocType,
                    StartPage = doc.StartPage,
                    EndPage = doc.EndPage
                })
                .ToList();
        }
    )
    .WithOpenApi();

app.MapPost(
        "/split",
        async Task (
            string blobName,
            string pageRange,
            IDocumentClassificationProvider provider,
            IPdfSplitter splitter
        ) =>
        {
            var result = await provider.ClassifyDocumentAsync(blobName, pageRange);
            await splitter.SplitPdfAsync(blobName, result);
        }
    )
    .WithOpenApi();

app.MapPost(
        "/extract/lessons",
        async Task<List<DocumentFields>> (
            string path,
            string modelId,
            ITextExtractionProvider provider
        ) =>
        {
            var urisResult = await provider.GetBlobsUrisAsync(path);
            return await provider.ExtractTextFromUrisAsync(urisResult, modelId);
        }
    )
    .WithOpenApi();

app.MapPost(
        "/extract/module-overview",
        async Task<List<DocumentFields>> (
            string uri,
            string modelId,
            ITextExtractionProvider provider
        ) =>
        {
            var results = await provider.ExtractTextFromUrisAsync(
                new List<Uri> { new Uri(uri) },
                modelId
            );
            results.ForEach(result =>
            {
                if (result.RawFields.Any(field => field.FieldName == "vocab"))
                {
                    var vocabFields = result.RawFields
                        .Where(f => f.FieldName == "vocab")
                        .Select(f => new ModuleOverviewField(
                            "vocab",
                            f.FieldContentRaw.Split(" · ")
                        ))
                        .ToList();
                    result.GetType().GetProperty("Fields")?.SetValue(result, vocabFields);
                }
            });

            return results;
        }
    )
    .WithOpenApi();

app.Run();
