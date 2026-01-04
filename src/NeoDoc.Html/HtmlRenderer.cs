using System.Text;
using NeoDoc.Core.Document;
using NeoDoc.Core.Nodes;
using NeoDoc.Core.Tables;

namespace NeoDoc.Html.Renderers;

internal sealed class HtmlRenderer : IHtmlRenderer
{
    public string Render(DocDocument document)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<body>");

        foreach (var node in document.Children)
        {
            RenderNode(node, sb);
        }

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private void RenderNode(DocNode node, StringBuilder sb)
    {
        switch (node)
        {
            case DocParagraph p:
                RenderParagraph(p, sb);
                break;

            case DocTable table:
                RenderTable(table, sb);
                break;
        }
    }

    private void RenderParagraph(DocParagraph paragraph, StringBuilder sb)
    {
        sb.Append("<p>");
        sb.Append(System.Net.WebUtility.HtmlEncode(paragraph.Text));
        sb.AppendLine("</p>");
    }

    private void RenderTable(DocTable table, StringBuilder sb)
    {
        sb.AppendLine("<table>");

        foreach (var row in table.Rows)
        {
            sb.AppendLine("<tr>");

            foreach (var cell in row.Cells)
            {
                RenderTableCell(cell, sb);
            }

            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</table>");
    }

    private void RenderTableCell(DocTableCell cell, StringBuilder sb)
    {
        sb.Append("<td");

        if (cell.ColSpan > 1)
            sb.Append($" colspan=\"{cell.ColSpan}\"");

        if (cell.RowSpan > 1)
            sb.Append($" rowspan=\"{cell.RowSpan}\"");

        sb.Append(">");

        foreach (var child in cell.Children)
        {
            RenderNode(child, sb);
        }

        sb.AppendLine("</td>");
    }
}
