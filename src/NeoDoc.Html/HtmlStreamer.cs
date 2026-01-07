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
        writer.Write(System.Net.WebUtility.HtmlEncode(paragraph.Text));
        writer.WriteLine("</p>");
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
