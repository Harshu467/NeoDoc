using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using NeoDoc.Core.Document;
using NeoDoc.Core.Nodes;
using NeoDoc.Core.Tables;

namespace NeoDoc.Docx.Parsers;

internal sealed class DocxParser : IDocxParser
{
    private MainDocumentPart? _mainPart;

    public DocDocument Parse(string filePath)
    {
        var document = new DocDocument();

        using var wordDoc = WordprocessingDocument.Open(filePath, false);
        _mainPart = wordDoc.MainDocumentPart;
        var body = _mainPart?.Document.Body;

        if (body == null)
            return document;

        foreach (var element in body.Elements())
        {
            if (element is Paragraph p)
                document.AddChild(ParseParagraph(p));
            else if (element is Table t)
                document.AddChild(ParseTable(t, _mainPart));
        }

        // Best-effort: attach any image parts to the first paragraph if images exist but were not inlined
        if (_mainPart != null && _mainPart.ImageParts.Any())
        {
            var firstPara = document.Children.OfType<DocParagraph>().FirstOrDefault();
            if (firstPara != null && !firstPara.Children.Any())
            {
                try
                {
                    var imgPart = _mainPart.ImageParts.First();
                    using var s = imgPart.GetStream();
                    using var ms = new MemoryStream();
                    s.CopyTo(ms);
                    var img = new DocImage
                    {
                        Data = ms.ToArray(),
                        ContentType = imgPart.ContentType,
                        Name = Path.GetFileName(imgPart.Uri?.ToString())
                    };

                    firstPara.AddChild(img);
                }
                catch
                {
                    // ignore
                }
            }
        }

        // clear reference
        _mainPart = null;

        return document;
    }

    private DocParagraph ParseParagraph(Paragraph paragraph)
    {
        var docParagraph = new DocParagraph();

        foreach (var run in paragraph.Elements<Run>())
        {
            // Handle text runs
            var docRun = new DocRun { Text = run.InnerText ?? string.Empty };

            if (run.RunProperties != null)
            {
                docRun.Bold = run.RunProperties.Bold != null;
                docRun.Italic = run.RunProperties.Italic != null;
                docRun.Underline = run.RunProperties.Underline != null && run.RunProperties.Underline.Val != null && run.RunProperties.Underline.Val != DocumentFormat.OpenXml.Wordprocessing.UnderlineValues.None;
                docRun.StyleId = run.RunProperties.RunStyle?.Val?.Value;
            }

            docParagraph.Runs.Add(docRun);

            // Handle inline images in the run (if any)
            var blip = run.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().FirstOrDefault();
            if (blip?.Embed != null && _mainPart != null)
            {
                try
                {
                    var relId = blip.Embed.Value;
                    var imgPart = _mainPart.GetPartById(relId) as ImagePart;
                    if (imgPart != null)
                    {
                        using var s = imgPart.GetStream();
                        using var ms = new MemoryStream();
                        s.CopyTo(ms);
                        var img = new DocImage
                        {
                            Data = ms.ToArray(),
                            ContentType = imgPart.ContentType,
                            Name = Path.GetFileName(imgPart.Uri?.ToString())
                        };

                        // Attach image to the specific run
                        docRun.AddChild(img);
                    }
                }
                catch
                {
                    // ignore image extraction errors for now
                }
            }
            else if (_mainPart != null)
            {
                // Fallback: search for any attribute named 'embed' in run descendants
                var embedAttr = run.Descendants().SelectMany(d => d.GetAttributes()).FirstOrDefault(a => a.LocalName == "embed" && !string.IsNullOrEmpty(a.Value));
                if (embedAttr != null)
                {
                    try
                    {
                        var relId = embedAttr.Value;
                        var imgPart = _mainPart.GetPartById(relId) as ImagePart;
                        if (imgPart != null)
                        {
                            using var s = imgPart.GetStream();
                            using var ms = new MemoryStream();
                            s.CopyTo(ms);
                            var img = new DocImage
                            {
                                Data = ms.ToArray(),
                                ContentType = imgPart.ContentType,
                                Name = Path.GetFileName(imgPart.Uri?.ToString())
                            };

                            docRun.AddChild(img);
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
        }

        // Fallback: if an image blip exists anywhere in paragraph, extract it and attach to the run that contains it if possible
        var paragraphBlip = paragraph.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().FirstOrDefault();
        if (paragraphBlip?.Embed != null && _mainPart != null)
        {
            try
            {
                var relId = paragraphBlip.Embed.Value;
                var imgPart = _mainPart.GetPartById(relId) as ImagePart;
                if (imgPart != null)
                {
                    using var s = imgPart.GetStream();
                    using var ms = new MemoryStream();
                    s.CopyTo(ms);
                    var img = new DocImage
                    {
                        Data = ms.ToArray(),
                        ContentType = imgPart.ContentType,
                        Name = Path.GetFileName(imgPart.Uri?.ToString())
                    };

                    var runs = paragraph.Elements<Run>().ToList();
                    var runWithBlip = runs.FirstOrDefault(r => r.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().Any());
                    if (runWithBlip != null)
                    {
                        var idx = runs.IndexOf(runWithBlip);
                        if (idx >= 0 && idx < docParagraph.Runs.Count)
                            docParagraph.Runs[idx].AddChild(img);
                        else
                        {
                            if (docParagraph.Runs.Any())
                                docParagraph.Runs.First().AddChild(img);
                            else
                                docParagraph.AddChild(img);
                        }
                    }
                    else if (docParagraph.Runs.Any())
                    {
                        docParagraph.Runs.First().AddChild(img);
                    }
                    else
                    {
                        docParagraph.AddChild(img);
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        // Fallback 1: attempt to find 'embed' attributes in the paragraph XML and attach referenced images
        if (docParagraph.Children.Count == 0 && _mainPart != null)
        {
            var xml = paragraph.OuterXml ?? string.Empty;
            var m = Regex.Match(xml, "embed\\s*=\\s*\"(?<id>[^\\\"]+)\"");
            if (m.Success)
            {
                var relId = m.Groups["id"].Value;
                try
                {
                    var imgPart = _mainPart.GetPartById(relId) as ImagePart;
                    if (imgPart != null)
                    {
                        using var s = imgPart.GetStream();
                        using var ms = new MemoryStream();
                        s.CopyTo(ms);
                        var img = new DocImage
                        {
                            Data = ms.ToArray(),
                            ContentType = imgPart.ContentType,
                            Name = Path.GetFileName(imgPart.Uri?.ToString())
                        };

                        // Attach to run that references this relId when possible
                        var runs = paragraph.Elements<Run>().ToList();
                        var runWithRel = runs.FirstOrDefault(r => r.Descendants().SelectMany(d => d.GetAttributes()).Any(a => a.LocalName == "embed" && a.Value == relId) || r.OuterXml.Contains(relId));
                        if (runWithRel != null)
                        {
                            var idx = runs.IndexOf(runWithRel);
                            if (idx >= 0 && idx < docParagraph.Runs.Count)
                                docParagraph.Runs[idx].AddChild(img);
                            else
                            {
                                if (docParagraph.Runs.Any())
                                    docParagraph.Runs.First().AddChild(img);
                                else
                                    docParagraph.AddChild(img);
                            }
                        }
                        else if (docParagraph.Runs.Any())
                        {
                            docParagraph.Runs.First().AddChild(img);
                        }
                        else
                        {
                            docParagraph.AddChild(img);
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }

        // Fallback 2: attach the first image part to the first run if available (best-effort)
        if (docParagraph.Children.Count == 0 && _mainPart != null && _mainPart.ImageParts.Any())
        {
            try
            {
                var imgPart = _mainPart.ImageParts.First();
                using var s = imgPart.GetStream();
                using var ms = new MemoryStream();
                s.CopyTo(ms);
                var img = new DocImage
                {
                    Data = ms.ToArray(),
                    ContentType = imgPart.ContentType,
                    Name = Path.GetFileName(imgPart.Uri?.ToString())
                };

                if (docParagraph.Runs.Any())
                    docParagraph.Runs.First().AddChild(img);
                        else
                        {
                            if (docParagraph.Runs.Any())
                                docParagraph.Runs.First().AddChild(img);
                            else
                                docParagraph.AddChild(img);
                        }
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

        // ColSpan
        var gridSpan = cell.TableCellProperties?
            .Elements<GridSpan>()
            .FirstOrDefault();

        if (gridSpan?.Val != null)
            docCell.ColSpan = (int)gridSpan.Val.Value;

        // RowSpan (basic vertical merge support)
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
