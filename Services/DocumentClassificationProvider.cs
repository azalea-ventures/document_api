using Azure;
using Azure.AI.DocumentIntelligence;

public interface IDocumentClassificationProvider
{
    Task<List<(string DocType, int StartPage, int EndPage)>> ClassifyDocumentsAsync(
        string blobUri
    );
    Task<List<(string DocType, int StartPage, int EndPage)>> ClassifyDocumentAsync(string blobUri, string pagesString);
}

public class DocumentClassificationProvider : IDocumentClassificationProvider
{
    private readonly IConfiguration _configuration;
    private readonly DocumentIntelligenceClient _client;

    public DocumentClassificationProvider(IConfiguration configuration)
    {
        _configuration = configuration;
        _client = new DocumentIntelligenceClient(
            new Uri(_configuration.GetValue<string>("AZURE_DOCUMENT_INTELLIGENCE_ENDPOINT")),
            new AzureKeyCredential(_configuration.GetValue<string>("AZURE_DOCUMENT_INTELLIGENCE_KEY"))
        );
    }

    public async Task<List<(string DocType, int StartPage, int EndPage)>> ClassifyDocumentsAsync(
        string blobUri
    )
    {
        Uri documentUri = new Uri(blobUri);

        var content = new AnalyzeDocumentContent() { UrlSource = documentUri };
        // Get total page count
        var analyzeOp = await _client.AnalyzeDocumentAsync(
            WaitUntil.Completed,
            "prebuilt-layout",
            content
        );
        int totalPages = analyzeOp.Value.Pages.Count;

        // Create batches of 100 pages
        var batches = Enumerable.Range(0, (totalPages + 99) / 100)
            .Select(i => $"{i * 100 + 1}-{Math.Min((i + 1) * 100, totalPages)}")
            .ToList();

        // Process batches in parallel
        var tasks = batches.Select(batch => ClassifyDocumentAsync(blobUri, batch));
        var results = await Task.WhenAll(tasks);

        // Combine all results
        return results.SelectMany(x => x).ToList();
    }

    public async Task<List<(string DocType, int StartPage, int EndPage)>> ClassifyDocumentAsync(
        string blobUri,
        string pagesString = ""
    )
    {
        Uri documentUri = new Uri(blobUri);

        var content = new ClassifyDocumentContent() { UrlSource = documentUri };

        Operation<AnalyzeResult> operation = await _client.ClassifyDocumentAsync(
            WaitUntil.Completed,
            _configuration.GetValue<string>("AZURE_DOCUMENT_CLASSIFIER_MODEL_ID"),
            content,
            split: SplitMode.Auto,
            pages: pagesString
        );
        AnalyzeResult result = operation.Value;

        List<(string DocType, IEnumerable<int> Pages)> docsPages = result
            .Documents.Select(doc =>
                (doc.DocType, doc.BoundingRegions.Select(reg => reg.PageNumber))
            )
            .ToList();

        List<(string DocType, int StartPage, int EndPage)> docs = docsPages
            .Select(
                (doc) =>
                {
                    return (doc.DocType, StartPage: doc.Pages.Min(), EndPage: doc.Pages.Max());
                }
            )
            .ToList();

        return docs;
    }
}
