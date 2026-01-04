using NeoDoc.Core.Document;
using NeoDoc.Docx;
using NeoDoc.Html;
using NeoDoc.Rules;
using NeoDoc.Core.Exceptions;

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
        try
        {
            var doc = DocxDocumentLoader.Load(docxPath);
            return new Document(doc);
        }
        catch (NeoDocException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new NeoDocException("Unexpected error while loading document.", ex);
        }
    }


    public Document ApplyRules(params IDocRule[] rules)
    {
        RuleProcessor.Apply(_doc, rules);
        return this;
    }


    public void Save(string htmlPath)
    {
        var html = HtmlDocumentWriter.Write(_doc);
        File.WriteAllText(htmlPath, html);
    }
}
