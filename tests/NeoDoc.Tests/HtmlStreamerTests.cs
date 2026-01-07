using System.IO;
using NeoDoc.Core.Document;
using NeoDoc.Core.Nodes;
using NeoDoc.Core.Tables;
using NeoDoc.Html;
using Xunit;

namespace NeoDoc.Tests;

public class HtmlStreamerTests
{
    [Fact]
    public void HtmlStreamer_WritesParagraphAndTable()
    {
        var doc = new DocDocument();

        var p = new DocParagraph { Text = "Hello, NeoDoc" };
        doc.AddChild(p);

        var table = new DocTable();
        var row = new DocTableRow();
        var cell = new DocTableCell();
        var inner = new DocParagraph { Text = "Cell text" };
        cell.AddChild(inner);
        row.Cells.Add(cell);
        table.Rows.Add(row);
        doc.AddChild(table);

        using var sw = new StringWriter();
        HtmlStreamer.Write(doc, sw);
        var output = sw.ToString();

        Assert.Contains("<!DOCTYPE html>", output);
        Assert.Contains("<p>", output);
        Assert.Contains("Hello, NeoDoc", output);
        Assert.Contains("<table>", output);
        Assert.Contains("Cell text", output);
    }

    [Fact]
    public void Document_Save_WritesToStream()
    {
        var doc = new DocDocument();
        doc.AddChild(new DocParagraph { Text = "Streamed" });

        using var ms = new MemoryStream();
        var document = new NeoDoc.Document(doc);
        document.Save(ms);
        ms.Position = 0;
        using var sr = new StreamReader(ms);
        var outStr = sr.ReadToEnd();

        Assert.Contains("Streamed", outStr);
        Assert.Contains("<!DOCTYPE html>", outStr);
    }
}
