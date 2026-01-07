using System;
using System.IO;
using NeoDoc.Core.Document;
using NeoDoc.Core.Nodes;
using NeoDoc.Core.Tables;

namespace NeoDoc.Html;

public static class HtmlStreamer
{
    public static void Write(DocDocument document, TextWriter writer)
    {
        writer.WriteLine("<!DOCTYPE html>");
        writer.WriteLine("<html>");
        writer.WriteLine("<body>");

        foreach (var node in document.Children)
        {
            RenderNode(node, writer);
        }

        writer.WriteLine("</body>");
        writer.WriteLine("</html>");
    }

    private static void RenderNode(DocNode node, TextWriter writer)
    {
        switch (node)
        {
            case DocParagraph p:
                RenderParagraph(p, writer);
                break;

            case DocTable table:
                RenderTable(table, writer);
                break;
        }
    }

    private static void RenderParagraph(DocParagraph paragraph, TextWriter writer)
    {
        writer.Write("<p>");

        if (paragraph.Runs != null && paragraph.Runs.Count > 0)
        {
            foreach (var run in paragraph.Runs)
            {
                RenderRun(run, writer);
            }
        }
        else
        {
            writer.Write(System.Net.WebUtility.HtmlEncode(paragraph.Text));
        }

        foreach (var child in paragraph.Children)
        {
            if (child is DocImage img)
            {
                var base64 = Convert.ToBase64String(img.Data);
                writer.Write($"<img src=\"data:{img.ContentType};base64,{base64}\" alt=\"{System.Net.WebUtility.HtmlEncode(img.Name ?? string.Empty)}\">");
            }
        }

        writer.WriteLine("</p>");
    }

    private static void RenderRun(DocRun run, TextWriter writer)
    {
        var encoded = System.Net.WebUtility.HtmlEncode(run.Text);

        if (run.Bold)
            writer.Write("<strong>");

        if (run.Italic)
            writer.Write("<em>");

        if (run.Underline)
            writer.Write("<u>");

        writer.Write(encoded);

        if (run.Underline)
            writer.Write("</u>");

        if (run.Italic)
            writer.Write("</em>");

        if (run.Bold)
            writer.Write("</strong>");
    }

    private static void RenderTable(DocTable table, TextWriter writer)
    {
        writer.WriteLine("<table>");

        foreach (var row in table.Rows)
        {
            writer.WriteLine("<tr>");

            foreach (var cell in row.Cells)
            {
                RenderTableCell(cell, writer);
            }

            writer.WriteLine("</tr>");
        }

        writer.WriteLine("</table>");
    }

    private static void RenderTableCell(DocTableCell cell, TextWriter writer)
    {
        writer.Write("<td");

        if (cell.ColSpan > 1)
            writer.Write($" colspan=\"{cell.ColSpan}\"");

        if (cell.RowSpan > 1)
            writer.Write($" rowspan=\"{cell.RowSpan}\"");

        writer.Write(">");

        foreach (var child in cell.Children)
        {
            RenderNode(child, writer);
        }

        writer.WriteLine("</td>");
    }
}
