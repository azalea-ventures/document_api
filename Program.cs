using System.Text.Json;
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

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost(
        "/classify",
        async Task<List<Document>> (string path, IDocumentClassificationProvider provider) =>
        {
            var result = await provider.ClassifyDocumentAsync(path);

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
            IDocumentClassificationProvider provider,
            IPdfSplitter splitter
        ) =>
        {
            await provider.ClassifyDocumentAsync(blobName);
            // await splitter.SplitPdfAsync(blobName,  )
        }
    )
    .WithOpenApi();

app.Run();
