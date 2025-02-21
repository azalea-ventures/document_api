using Azure.Storage.Blobs;
using DocumentApi.Data.content;
using DocumentApi.Services;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDocumentClassificationProvider, DocumentClassificationProvider>();

string? blobConnectionString =
    Environment.GetEnvironmentVariable("REVISA_BUCKET")
    ?? builder.Configuration.GetConnectionString("REVISA_BUCKET");
builder.Services.AddSingleton(x => new BlobServiceClient(blobConnectionString));

// Database config setup
string? connectionString =
    Environment.GetEnvironmentVariable("REVISA_DB")
    ?? builder.Configuration.GetConnectionString("REVISA_DB");

Action<DbContextOptionsBuilder> dbConfig = (opt) =>
{
    opt.UseSqlServer(connectionString);
    // opt.EnableSensitiveDataLogging(true);
};

builder.Services.AddDbContext<ContentContext>(dbConfig);
builder.Services.AddScoped<IPdfSplitter, PdfSplitter>();
builder.Services.AddScoped<ITextExtractionProvider, TextExtractionProvider>();
builder.Services.AddScoped<IContentFieldService, ContentFieldService>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost(
        "/classify",
        async Task<List<Document>> (string documentUri, IDocumentClassificationProvider provider) =>
        {
            var result = await provider.ClassifyDocumentsAsync(documentUri);

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
            string documentUri,
            List<string> desiredDocTypes,
            IDocumentClassificationProvider provider,
            IPdfSplitter splitter
        ) =>
        {
            var result = await provider.ClassifyDocumentsAsync(documentUri);
            await splitter.SplitPdfAsync(documentUri, desiredDocTypes, result);
        }
    )
    .WithOpenApi();

app.MapPost(
        "/extract/lessons",
        async Task<List<DocumentFields>> (string path, ITextExtractionProvider provider) =>
        {
            var textResults = await provider.ExtractTextFromUrisAsync(
                await provider.GetBlobsUrisAsync(path)
            );

            return
            [
                .. textResults.OrderBy(r =>
                {
                    var lessonField = r.RawFields.FirstOrDefault(f =>
                        f.FieldName == "lessonNumber"
                    );
                    if (lessonField == null || string.IsNullOrEmpty(lessonField.FieldContentRaw))
                    {
                        return int.MaxValue; // Place records without a lesson number at the end
                    }

                    // Extract numeric value from FieldContentRaw
                    string numericContent = new string(
                        lessonField.FieldContentRaw.Where(char.IsDigit).ToArray()
                    );

                    if (int.TryParse(numericContent, out int lessonNumber))
                    {
                        lessonField.FieldContentRaw = lessonNumber.ToString(); // Reassign in normalized form
                        return lessonNumber;
                    }

                    return int.MaxValue; // If parsing fails, place it at the end
                })
            ];
        }
    )
    .WithOpenApi();

app.MapPost(
        "/extract/module-overview",
        async Task<List<DocumentFields>> (
            string uri,
            ITextExtractionProvider provider,
            IContentFieldService contentFieldService
        ) =>
        {
            // Check if a record exists with the same SourceContentName
            SourceContent existingSourceContent = await contentFieldService.GetSourceContentByPath(
                uri
            );
            if (existingSourceContent != null)
            {
                // Return the existing record
                return new List<DocumentFields>
                {
                    new DocumentFields
                    {
                        RawFields = existingSourceContent
                            .SourceContentFields.Select(f => new FieldBase(
                                f.FieldName,
                                f.FieldContent
                            ))
                            .ToList()
                    }
                };
            }

            //extract from blob file
            var results = await provider.ExtractTextFromUrisAsync(new List<Uri> { new Uri(uri) });

            foreach (var result in results)
            {
                if (result.RawFields.Any(field => field.FieldName == "vocab"))
                {
                    result.ModuleOverviewFields = result
                        .RawFields.Where(f => f.FieldName == "vocab")
                        .Select(f => new ModuleOverviewField(
                            "vocab",
                            f.FieldContentRaw.Split(" · ")
                        ))
                        .ToList();
                }

                // Call PostModuleOverviewFields method
                var postResult = await contentFieldService.PostModuleOverviewFields(
                    result.RawFields,
                    uri,
                    "EUREKA"
                );

                if (postResult != "success")
                {
                    throw new Exception(postResult);
                }
            }

            return results;
        }
    )
    .WithOpenApi();

app.Run();
