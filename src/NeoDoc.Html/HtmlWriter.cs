using System.IO;
using NeoDoc.Core.Document;

namespace NeoDoc.Html;

public sealed class HtmlWriter : Core.Document.IDocumentWriter
{
    public void Write(DocDocument document, TextWriter writer)
    {
        HtmlStreamer.Write(document, writer);
    }
}
