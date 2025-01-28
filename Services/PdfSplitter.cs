using System.Text;
using Azure.Storage.Blobs;
using iText.Kernel.Pdf;

public interface IPdfSplitter
{
    Task SplitPdfAsync(
        string blobUri,
        string desiredDocType,
        List<(string DocType, int StartPage, int EndPage)> documents
    );
}

public class PdfSplitter : IPdfSplitter
{
    private readonly BlobContainerClient _containerClient;

    public PdfSplitter(BlobServiceClient blobServiceClient)
    {
        _containerClient = blobServiceClient.GetBlobContainerClient("source-files");
        _containerClient.CreateIfNotExists();
    }

    public async Task SplitPdfAsync(
        string blobUri,
        string desiredDocType,
        List<(string DocType, int StartPage, int EndPage)> documents
    )
    {
        string destFolder = "output";

        // Extract blob name from the URI
        string inboundBlobName = string.Join(
            "/",
            blobUri.Split("/").Reverse().Take(2).Reverse().ToArray()
        );

        BlobClient blobClient = _containerClient.GetBlobClient(inboundBlobName);

        using (MemoryStream stream = new MemoryStream())
        {
            await blobClient.DownloadToAsync(stream);
            stream.Position = 0;

            int docNumber = 0;

            foreach (var document in documents)
            {
                var (docType, startPage, endPage) = document;

                using (MemoryStream splitDocStream = new MemoryStream())
                {
                    SetOutputStream(stream, splitDocStream, startPage, endPage);
                    splitDocStream.Position = 0;

                    string outboundblobName = destFolder;

                    if (docType.Equals(desiredDocType, StringComparison.OrdinalIgnoreCase))
                    {
                        docNumber++;
                        outboundblobName += $"/{docType}{docNumber}.pdf";
                    }

                    BlobClient partBlobClient = _containerClient.GetBlobClient(outboundblobName);

                    await partBlobClient.UploadAsync(splitDocStream, overwrite: true);
                }
            }
        }
    }

    private void SetOutputStream(
        Stream documentStream,
        MemoryStream outputStream,
        int startPage,
        int endPage
    )
    {
        using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(documentStream)))
        using (MemoryStream tempStream = new MemoryStream())
        using (PdfDocument splitDoc = new PdfDocument(new PdfWriter(tempStream)))
        {
            List<int> pagesToCopy = new List<int>();
            for (int i = startPage; i <= endPage; i++)
            {
                pagesToCopy.Add(i);
            }

            pdfDoc.CopyPagesTo(pagesToCopy, splitDoc); // love this
            pdfDoc.SetCloseReader(false); // keep the source stream open after pdf doc closure
            documentStream.Position = 0; // make sure the source stream is reset before the next split doc is written
            // Close splitDoc to finalize the PDF content in tempStream
            splitDoc.SetCloseWriter(false);
            splitDoc.Close();
            // Copy the finalized PDF content from tempStream to the provided writeStream
            tempStream.Position = 0;
            tempStream.CopyTo(outputStream);
        }

        // Reset the position of writeStream to the beginning for reading
        outputStream.Position = 0;
    }
}
