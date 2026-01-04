using NeoDoc.Core.Document;

namespace NeoDoc.Html.Renderers;

internal interface IHtmlRenderer
{
    string Render(DocDocument document);
}
