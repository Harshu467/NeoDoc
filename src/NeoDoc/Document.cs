using NeoDoc.Core.Document;
using NeoDoc.Docx;
using NeoDoc.Html;
using NeoDoc.Rules;
using NeoDoc.Core.Exceptions;

namespace NeoDoc;

public sealed class Document
{
    private readonly DocDocument _doc;

    public Document(DocDocument doc)
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

    public static IDocumentReader OpenReader(string docxPath)
    {
        return new Docx.DocxDocumentReader(docxPath);
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

    public void SaveStreamed(string htmlPath)
    {
        try
        {
            using var fs = new FileStream(htmlPath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var sw = new StreamWriter(fs);
            HtmlStreamer.Write(_doc, sw);
        }
        catch (Exception ex)
        {
            throw new NeoDocException("Unexpected error while saving streamed HTML.", ex);
        }
    }

    public void Save(Stream stream)
    {
        try
        {
            using var sw = new StreamWriter(stream, leaveOpen: true);
            var writer = new NeoDoc.Html.HtmlWriter();
            writer.Write(_doc, sw);
            sw.Flush();
        }
        catch (Exception ex)
        {
            throw new NeoDocException("Unexpected error while saving HTML to stream.", ex);
        }
    }
}
