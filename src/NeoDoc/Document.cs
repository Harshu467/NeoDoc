using NeoDoc.Core.Document;
using NeoDoc.Docx;
using NeoDoc.Html;
using NeoDoc.Rules;

namespace NeoDoc;

public sealed class Document
{
    private readonly DocDocument _doc;

    private Document(DocDocument doc)
    {
        _doc = doc;
    }

    public static Document Load(string docxPath)
    {
        var doc = DocxDocumentLoader.Load(docxPath);
        return new Document(doc);
    }

    public void ApplyRules(params IDocRule[] rules)
    {
        RuleProcessor.Apply(_doc, rules);
    }

    public void Save(string htmlPath)
    {
        var html = HtmlDocumentWriter.Write(_doc);
        File.WriteAllText(htmlPath, html);
    }
}
