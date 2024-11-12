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
        "/extract",
        async Task<List<DocumentFields>> (string path, ITextExtractionProvider provider) =>
        {
            var urisResult = await provider.GetBlobsUrisAsync(path);
            return await provider.ExtractTextFromUrisAsync(urisResult);
        }
    )
    .WithOpenApi();

app.Run();
