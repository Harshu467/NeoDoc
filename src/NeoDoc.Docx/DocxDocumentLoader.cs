using NeoDoc.Core.Document;
using NeoDoc.Core.Exceptions;
using NeoDoc.Docx.Parsers;

namespace NeoDoc.Docx;

public static class DocxDocumentLoader
{
    public static DocDocument Load(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new InvalidDocumentException("Document path is null or empty.");

        if (!File.Exists(filePath))
            throw new InvalidDocumentException($"Document not found: {filePath}");

        var fileInfo = new FileInfo(filePath);

        if (fileInfo.Length == 0)
            throw new InvalidDocumentException("The DOCX file is empty.");

        try
        {
            IDocxParser parser = new DocxParser();
            return parser.Parse(filePath);
        }
        catch (Exception ex)
        {
            throw new InvalidDocumentException(
                "Failed to load DOCX document. The file may be corrupted or not a valid Word document.",
                ex
            );
        }
    }
}
