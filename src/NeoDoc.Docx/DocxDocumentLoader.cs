using NeoDoc.Core.Document;
using NeoDoc.Docx.Parsers;

namespace NeoDoc.Docx;

public static class DocxDocumentLoader
{
    public static DocDocument Load(string filePath)
    {
        IDocxParser parser = new DocxParser();
        return parser.Parse(filePath);
    }
}
