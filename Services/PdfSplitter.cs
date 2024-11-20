using System.Text;
using Azure.Storage.Blobs;
using iText.Kernel.Pdf;

public interface IPdfSplitter
{
    Task SplitPdfAsync(
        string blobUri,
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
        List<(string DocType, int StartPage, int EndPage)> documents
    )
    {
        string destFolder = "output";
        string blobName = String.Join(
            "/",
            blobUri.Split("/").Reverse().Take(2).Reverse().ToArray()
        );
        // Get a reference to the blob (original full document)
        BlobClient blobClient = _containerClient.GetBlobClient(blobName);
        // Download the blob to a stream
        using (MemoryStream stream = new MemoryStream())
        {
            await blobClient.DownloadToAsync(stream);
            stream.Position = 0; // Reset the stream position
            List<(int StartPage, int EndPage)> pageBoundaries = documents
                .Select(doc => (doc.StartPage, doc.EndPage))
                .ToList();

            // Loop through each boundary and save each part as a separate blob

            string topic = "";
            int lessonNumber = 1;
            for (int i = 0; i < pageBoundaries.Count; i++)
            {
                var (startPage, endPage) = pageBoundaries[i];

                using (MemoryStream splitDocStream = new MemoryStream())
                {
                    SetOutputStream(stream, splitDocStream, startPage, endPage);
                    splitDocStream.Position = 0;
                    // Save the part to a new blob
                    string docTypeFolder = documents[i].DocType;

                    if (docTypeFolder == "topic")
                    {
                        lessonNumber = 1;
                        switch (topic)
                        {
                            case "":
                                topic = "A";
                                break;
                            case "A":
                                topic = "B";
                                break;
                            case "B":
                                topic = "C";
                                break;
                            case "C":
                                topic = "D";
                                break;
                            case "D":
                                topic = "E";
                                break;
                            case "E":
                                topic = "F";
                                break;
                            case "F":
                                topic = "G";
                                break;
                            case "G":
                                topic = "H";
                                break;
                            case "H":
                                topic = "I";
                                break;
                            case "I":
                                topic = "J";
                                break;
                            case "J":
                                topic = "K";
                                break;
                            case "K":
                                topic = "L";
                                break;
                            case "L":
                                topic = "M";
                                break;
                            default:
                                topic = "N";
                                break;
                        }
                    }
                    if (docTypeFolder == "lesson"){
                        lessonNumber++;
                    }

                    string partBlobName = $"{destFolder}/{(topic == "" ? "meta" : topic)}/{docTypeFolder}/{lessonNumber}.pdf";
                    BlobClient partBlobClient = _containerClient.GetBlobClient(partBlobName);

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
