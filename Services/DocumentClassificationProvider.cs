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

        private IConfiguration _configuration;

        public DocumentClassificationProvider(IConfiguration configuration){
            _configuration = configuration;
        }
    public async Task<List<(string DocType, int StartPage, int EndPage)>> ClassifyDocumentAsync(
        string blobName, string pageRange
    )
    {

        var client = new DocumentIntelligenceClient(
            new Uri(_configuration.GetValue<string>("AZURE_DOCUMENT_INTELLIGENCE_ENDPOINT")),
            new AzureKeyCredential(_configuration.GetValue<string>("AZURE_DOCUMENT_INTELLIGENCE_KEY"))
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
