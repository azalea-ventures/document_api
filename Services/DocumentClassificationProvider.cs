using Azure;
using Azure.AI.DocumentIntelligence;

public interface IDocumentClassificationProvider
{
    Task<List<(string DocType, int StartPage, int EndPage)>> ClassifyDocumentAsync(
        string blobName, string pageRange
    );
}

public class DocumentClassificationProvider : IDocumentClassificationProvider
{
    public async Task<List<(string DocType, int StartPage, int EndPage)>> ClassifyDocumentAsync(
        string blobName, string pageRange
    )
    {
        string endpoint = "https://revisa-reader-ai.cognitiveservices.azure.com/";
        string apiKey = "4aa34c211f434dd0a52bcdf69a27fc3d";

        var client = new DocumentIntelligenceClient(
            new Uri(endpoint),
            new AzureKeyCredential(apiKey)
        );

        Uri documentUri = new Uri(blobName);
        string classifierId = "revisa-pdf-mapperv0.4.0";
        var content = new ClassifyDocumentContent() { UrlSource = documentUri };

        Operation<AnalyzeResult> operation = await client.ClassifyDocumentAsync(
            WaitUntil.Completed,
            classifierId,
            content,
            split: SplitMode.Auto,
            pages: pageRange
        );
        AnalyzeResult result = operation.Value;

        List<(string DocType, IEnumerable<int> Pages)> docsPages = result
            .Documents.Select(doc =>
                (doc.DocType, doc.BoundingRegions.Select(reg => reg.PageNumber))
            )
            .ToList();

        List<(string DocType, int StartPage, int EndPage)> docs = docsPages.Select((doc) => {
            return (DocType:doc.DocType, StartPage:doc.Pages.Min(), EndPage:doc.Pages.Max());
            }).ToList();

        return docs;
    }
}
