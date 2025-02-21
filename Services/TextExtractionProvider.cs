using Azure;
using Azure.AI.DocumentIntelligence;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

public interface ITextExtractionProvider
{
    Task<List<Uri>> GetBlobsUrisAsync(string path);
    Task<List<DocumentFields>> ExtractTextFromUrisAsync(List<Uri> blobUris);
}

public class TextExtractionProvider : ITextExtractionProvider
{
    private readonly BlobContainerClient _containerClient;
    private readonly IConfiguration _configuration;

    public TextExtractionProvider(BlobServiceClient blobServiceClient, IConfiguration configuration)
    {
        _containerClient = blobServiceClient.GetBlobContainerClient("source-files");
        _containerClient.CreateIfNotExists();
        _configuration = configuration;
    }

    public async Task<List<Uri>> GetBlobsUrisAsync(string path)
    {
        List<Uri> blobUris = new List<Uri>();

        // List blobs with the specified prefix (folder)
        await foreach (BlobItem blobItem in _containerClient.GetBlobsAsync(prefix: path))
        {
            // Create a BlobClient for each blob to access its Uri
            BlobClient blobClient = _containerClient.GetBlobClient(blobItem.Name);
            blobUris.Add(blobClient.Uri);
        }

        return blobUris;
    }

    public async Task<List<DocumentFields>> ExtractTextFromUrisAsync(List<Uri> blobUris)
    {
        if (!blobUris.Any())
        {
            return null;
        }

        return await Task.WhenAll(
                blobUris
                    .Select(
                        async (uri) =>
                        {
                            return await ExtractTextFromUriAsync(uri);
                        }
                    )
                    .ToArray()
            )
            .ContinueWith(
                (taskRes) =>
                {
                    List<DocumentFields> resultFields = new();

                    foreach (AnalyzeResult result in taskRes.Result)
                    {
                        foreach (AnalyzedDocument doc in result.Documents)
                        {
                            List<FieldBase> docFields = new();
                            docFields = doc
                                .Fields.Select(field => new FieldBase(
                                    field.Key,
                                    field.Value.Content
                                ))
                                .ToList();

                            resultFields.Add(new DocumentFields(docFields));
                        }
                    }

                    return resultFields;
                }
            );
    }

    private async Task<AnalyzeResult> ExtractTextFromUriAsync(Uri blobUri)
    {
        var client = new DocumentIntelligenceClient(
            new Uri(_configuration.GetValue<string>("AZURE_TEXT_EXTRACTOR_ENDPOINT")),
            new AzureKeyCredential(_configuration.GetValue<string>("AZURE_TEXT_EXTRACTOR_KEY"))
        );
        // Set your custom model ID
        var content = new AnalyzeDocumentContent() { UrlSource = blobUri };

        Operation<AnalyzeResult> operation = await client.AnalyzeDocumentAsync(
            WaitUntil.Completed,
            _configuration.GetValue<string>("AZURE_TEXT_EXTRACTOR_MODEL_ID"),
            content
        );
        AnalyzeResult result = operation.Value;

        return result;
    }
}
