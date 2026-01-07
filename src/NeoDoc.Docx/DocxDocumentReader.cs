using System.Collections.Generic;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using NeoDoc.Core.Document;
using NeoDoc.Core.Nodes;
using NeoDoc.Core.Tables;

namespace NeoDoc.Docx;

public sealed class DocxDocumentReader : IDocumentReader
{
    private readonly string _filePath;

    public DocxDocumentReader(string filePath)
    {
        _filePath = filePath;
    }

    public IEnumerable<DocNode> ReadNodes()
    {
        using var wordDoc = WordprocessingDocument.Open(_filePath, false);
        var body = wordDoc.MainDocumentPart?.Document.Body;

        if (body == null)
            yield break;

        foreach (var element in body.Elements())
        {
            if (element is Paragraph p)
                yield return ParseParagraph(p);
            else if (element is Table t)
                yield return ParseTable(t);
        }
    }

    private DocParagraph ParseParagraph(Paragraph paragraph)
    {
        var docParagraph = new DocParagraph();

        foreach (var run in paragraph.Elements<Run>())
        {
            docParagraph.Text += run.InnerText;
        }

        return docParagraph;
    }

    private DocTable ParseTable(Table table)
    {
        var docTable = new DocTable();

        foreach (var row in table.Elements<TableRow>())
        {
            var docRow = new DocTableRow();

            foreach (var cell in row.Elements<TableCell>())
            {
                docRow.Cells.Add(ParseTableCell(cell));
            }

            docTable.Rows.Add(docRow);
        }

        return docTable;
    }

    private DocTableCell ParseTableCell(TableCell cell)
    {
        var docCell = new DocTableCell();

        var gridSpan = cell.TableCellProperties?
            .Elements<GridSpan>()
            .FirstOrDefault();

        if (gridSpan?.Val != null)
            docCell.ColSpan = (int)gridSpan.Val.Value;

        var vMerge = cell.TableCellProperties?
            .Elements<VerticalMerge>()
            .FirstOrDefault();

        if (vMerge != null && vMerge.Val == null)
            docCell.RowSpan = 1;

        foreach (var element in cell.Elements<Paragraph>())
        {
            docCell.AddChild(ParseParagraph(element));
        }

        return docCell;
    }
}
