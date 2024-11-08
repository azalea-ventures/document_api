using System.Text;
using Azure.Storage.Blobs;

public interface IPdfSplitter
{
    Task SplitPdfAsync(string blobName, List<(int StartPage, int EndPage)> pageBoundaries);
}

public class PdfSplitter : IPdfSplitter
{
    private readonly BlobContainerClient _containerClient;

    public PdfSplitter(BlobServiceClient blobServiceClient, IConfiguration configuration)
    {
        _containerClient = blobServiceClient.GetBlobContainerClient(configuration["BlobContainerName"]);
        _containerClient.CreateIfNotExists();
    }

    public async Task SplitPdfAsync(
        string blobName,
        List<(int StartPage, int EndPage)> pageBoundaries
    )
    {
        // Get a reference to the blob (original full document)
        BlobClient blobClient = _containerClient.GetBlobClient(blobName);

        // Download the blob to a stream
        using (MemoryStream stream = new MemoryStream())
        {
            await blobClient.DownloadToAsync(stream);
            stream.Position = 0; // Reset the stream position

            // Loop through each boundary and save each part as a separate blob
            for (int i = 0; i < pageBoundaries.Count; i++)
            {
                var (startPage, endPage) = pageBoundaries[i];
                var extractedContent = ExtractPagesFromStream(stream, startPage, endPage);

                // Save the part to a new blob
                string partBlobName =
                    $"{Path.GetFileNameWithoutExtension(blobName)}_part{i + 1}.pdf";
                BlobClient partBlobClient = _containerClient.GetBlobClient(partBlobName);

                using (var partStream = new MemoryStream(Encoding.UTF8.GetBytes(extractedContent)))
                {
                    await partBlobClient.UploadAsync(partStream, overwrite: true);
                }
            }
        }
    }

    private string ExtractPagesFromStream(Stream documentStream, int startPage, int endPage)
    {
        // Placeholder for actual document processing to extract specific pages.
        // This would involve using a PDF library or processing library to split based on page numbers.

        // For example, here you could use PDF processing libraries like PdfSharp or iText7
        // to extract specific pages based on the startPage and endPage arguments.

        return "extracted content for specified pages"; // Return dummy text for example
    }
}
