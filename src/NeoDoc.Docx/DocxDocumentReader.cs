using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        var mainPart = wordDoc.MainDocumentPart;
        var body = mainPart?.Document.Body;

        if (body == null)
            yield break;

        foreach (var element in body.Elements())
        {
            if (element is Paragraph p)
                yield return ParseParagraph(p, mainPart);
            else if (element is Table t)
                yield return ParseTable(t, mainPart);
        }
    }

    private DocParagraph ParseParagraph(Paragraph paragraph, MainDocumentPart? mainPart)
    {
        var docParagraph = new DocParagraph();

        foreach (var run in paragraph.Elements<Run>())
        {
            var docRun = new DocRun { Text = run.InnerText ?? string.Empty };

            if (run.RunProperties != null)
            {
                docRun.Bold = run.RunProperties.Bold != null;
                docRun.Italic = run.RunProperties.Italic != null;
                docRun.Underline = run.RunProperties.Underline != null && run.RunProperties.Underline.Val != null && run.RunProperties.Underline.Val != DocumentFormat.OpenXml.Wordprocessing.UnderlineValues.None;
                docRun.StyleId = run.RunProperties.RunStyle?.Val?.Value;
            }

            docParagraph.Runs.Add(docRun);

            var blip = run.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().FirstOrDefault();
            if (blip?.Embed != null && mainPart != null)
            {
                try
                {
                    var relId = blip.Embed.Value;
                    var imgPart = mainPart.GetPartById(relId) as ImagePart;
                    if (imgPart != null)
                    {
                        using var s = imgPart.GetStream();
                        using var ms = new MemoryStream();
                        s.CopyTo(ms);
                        var img = new DocImage
                        {
                            Data = ms.ToArray(),
                            ContentType = imgPart.ContentType,
                            Name = imgPart.Uri?.Segments?.LastOrDefault()
                        };

                        docParagraph.AddChild(img);
                    }
                }
                catch
                {
                    // ignore
                }
            }
            else if (mainPart != null)
            {
                var embedAttr = run.Descendants().SelectMany(d => d.GetAttributes()).FirstOrDefault(a => a.LocalName == "embed" && !string.IsNullOrEmpty(a.Value));
                if (embedAttr != null)
                {
                    try
                    {
                        var relId = embedAttr.Value;
                        var imgPart = mainPart.GetPartById(relId) as ImagePart;
                        if (imgPart != null)
                        {
                            using var s = imgPart.GetStream();
                            using var ms = new MemoryStream();
                            s.CopyTo(ms);
                            var img = new DocImage
                            {
                                Data = ms.ToArray(),
                                ContentType = imgPart.ContentType,
                                Name = imgPart.Uri?.Segments?.LastOrDefault()
                            };

                            docParagraph.AddChild(img);
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
        }

        // Fallback: extract blip at paragraph level if not captured per-run
        var paragraphBlip = paragraph.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().FirstOrDefault();
        if (paragraphBlip?.Embed != null && mainPart != null)
        {
            try
            {
                var relId = paragraphBlip.Embed.Value;
                var imgPart = mainPart.GetPartById(relId) as ImagePart;
                if (imgPart != null)
                {
                    using var s = imgPart.GetStream();
                    using var ms = new MemoryStream();
                    s.CopyTo(ms);
                    var img = new DocImage
                    {
                        Data = ms.ToArray(),
                        ContentType = imgPart.ContentType,
                        Name = imgPart.Uri?.Segments?.LastOrDefault()
                    };

                    docParagraph.AddChild(img);
                }
            }
            catch
            {
                // ignore
            }
        }

        // Fallback 1: look for 'embed' attributes inside the paragraph XML and attach referenced images
        if (docParagraph.Children.Count == 0 && mainPart != null)
        {
            var xml = paragraph.OuterXml ?? string.Empty;
            var m = Regex.Match(xml, "embed\s*=\s*\"(?<id>[^"]+)\"");
            if (m.Success)
            {
                var relId = m.Groups["id"].Value;
                try
                {
                    var imgPart = mainPart.GetPartById(relId) as ImagePart;
                    if (imgPart != null)
                    {
                        using var s = imgPart.GetStream();
                        using var ms = new MemoryStream();
                        s.CopyTo(ms);
                        var img = new DocImage
                        {
                            Data = ms.ToArray(),
                            ContentType = imgPart.ContentType,
                            Name = imgPart.Uri?.Segments?.LastOrDefault()
                        };

                        docParagraph.AddChild(img);
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }

        // Fallback 2: attach the first image part if still empty
        if (docParagraph.Children.Count == 0 && mainPart != null && mainPart.ImageParts.Any())
        {
            try
            {
                var imgPart = mainPart.ImageParts.First();
                using var s = imgPart.GetStream();
                using var ms = new MemoryStream();
                s.CopyTo(ms);
                var img = new DocImage
                {
                    Data = ms.ToArray(),
                    ContentType = imgPart.ContentType,
                    Name = imgPart.Uri?.Segments?.LastOrDefault()
                };

                docParagraph.AddChild(img);
            }
            catch
            {
                // ignore
            }
        }

        docParagraph.UpdateTextFromRuns();

        return docParagraph;
    }

    private DocTable ParseTable(Table table, MainDocumentPart? mainPart)
    {
        var docTable = new DocTable();

        foreach (var row in table.Elements<TableRow>())
        {
            var docRow = new DocTableRow();

            foreach (var cell in row.Elements<TableCell>())
            {
                docRow.Cells.Add(ParseTableCell(cell, mainPart));
            }

            docTable.Rows.Add(docRow);
        }

        return docTable;
    }

    private DocTableCell ParseTableCell(TableCell cell, MainDocumentPart? mainPart)
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
            docCell.AddChild(ParseParagraph(element, mainPart));
        }

        return docCell;
    }
}
