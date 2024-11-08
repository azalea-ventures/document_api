using System;
using System.Collections.Generic;
using System.IO;
using Azure;
using Azure.AI.DocumentIntelligence;

public interface IDocumentClassificationProvider {
    Task ClassifyDocumentAsync(string documentLocation);
 }

public class DocumentClassificationProvider : IDocumentClassificationProvider
{
    // Classify Document

    // 1. (OPTIONAL) Verify can access document from provided URL

    // 2. Get document classification, split by document type, from Azure AI
    public async Task ClassifyDocumentAsync(string documentLocation)
    {
        string endpoint = "https://revisa-reader-ai.cognitiveservices.azure.com/";
        string apiKey = "4aa34c211f434dd0a52bcdf69a27fc3d";

        var client = new DocumentIntelligenceClient(
            new Uri(endpoint),
            new AzureKeyCredential(apiKey)
        );

        Uri documentUri = new Uri(documentLocation);
        string classifierId = "revisa-pdf-mapperv0.4.0";
        var content = new ClassifyDocumentContent() { UrlSource = documentUri };

        Operation<AnalyzeResult> operation = await client.ClassifyDocumentAsync(
            WaitUntil.Completed,
            classifierId,
            content,
            split:SplitMode.Auto,
            pages:"1-33"
        );
        AnalyzeResult result = operation.Value;

        Console.WriteLine($"Input was classified by the classifier with ID '{result.ModelId}'.");

// next, take each document and extract basic text from Text Extractor
        foreach (AnalyzedDocument document in result.Documents)
        {
            
        }
    }
    // 3. Get File from Azure Blob Storage

    // 4. Use AI Response and to split File by type (lesson, module overview, etc)
    // ----> will need to merge pages into single files and follow convention:
    // ---->  <module>_<topic>_<lesson>_<language>.pdf
    // --------> Lesson: M3_TB_L8_EN.pdf
    // --------> Topic: M3_TB_ES.pdf
    // --------> Module Overview: M3_ES.pdf


    // 5. Create Module folder and place newly split Files within

    // 4. Parse re
}
