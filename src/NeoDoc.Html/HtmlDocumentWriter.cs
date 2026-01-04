using NeoDoc.Core.Document;
using NeoDoc.Html.Renderers;

namespace NeoDoc.Html;

public static class HtmlDocumentWriter
{
    public static string Write(DocDocument document)
    {
        var renderer = new HtmlRenderer();
        return renderer.Render(document);
    }
}
