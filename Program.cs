using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDocumentClassificationProvider, DocumentClassificationProvider>();

// // Azure services registration
// string? blobConnectionString =
//     Environment.GetEnvironmentVariable("REVISA_BUCKET")
//     ?? builder.Configuration.GetConnectionString("REVISA_BUCKET");

// builder.Services.AddAzureClients(clientBuilder =>
// {
//     clientBuilder.AddBlobServiceClient(blobConnectionString);
// });

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "hi");

app.MapPost(
        "/classify",
        async Task (string documentLocation, IDocumentClassificationProvider provider) =>
            await provider.ClassifyDocumentAsync(documentLocation)
    );

app.Run();
