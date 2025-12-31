using NeoDoc.Core.Document;

namespace NeoDoc.Docx.Parsers;

internal interface IDocxParser
{
    DocDocument Parse(string filePath);
}
