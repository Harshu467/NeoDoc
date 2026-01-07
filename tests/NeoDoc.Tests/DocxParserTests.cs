using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using NeoDoc.Docx;
using NeoDoc.Core.Nodes;
using Xunit;

namespace NeoDoc.Tests;

public class DocxParserTests
{
    [Fact]
    public void DocxParser_ParsesRunsWithFormatting()
    {
        var tmp = System.IO.Path.GetTempFileName();
        File.Delete(tmp);
        var file = tmp + ".docx";

        using (var wordDoc = WordprocessingDocument.Create(file, WordprocessingDocumentType.Document))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new Body());
            var body = mainPart.Document.Body;

            var para = new Paragraph();
            var run1 = new Run(new Text("Hello "));
            var run2 = new Run();
            run2.Append(new RunProperties(new Bold()));
            run2.Append(new Text("Bold"));

            para.Append(run1);
            para.Append(run2);
            body.Append(para);
            mainPart.Document.Save();
        }

        var doc = DocxDocumentLoader.Load(file);

        Assert.NotEmpty(doc.Children);
        var paragraph = doc.Children.OfType<DocParagraph>().FirstOrDefault();
        Assert.NotNull(paragraph);
        Assert.True(paragraph.Runs.Count >= 2);
        Assert.Contains(paragraph.Runs, r => r.Bold && r.Text.Contains("Bold"));

        File.Delete(file);
    }

    [Fact]
    public void DocxParser_ParsesInlineImage()
    {
        var tmp = System.IO.Path.GetTempFileName();
        File.Delete(tmp);
        var file = tmp + ".docx";

        // minimal 1x1 PNG
        var pngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR4nGNgYAAAAAMAASsJTYQAAAAASUVORK5CYII=";
        var pngBytes = System.Convert.FromBase64String(pngBase64);

        using (var wordDoc = WordprocessingDocument.Create(file, WordprocessingDocumentType.Document))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new Body());
            var body = mainPart.Document.Body;

            var imagePart = mainPart.AddImagePart(ImagePartType.Png);
            using (var stream = imagePart.GetStream())
                stream.Write(pngBytes, 0, pngBytes.Length);

            var relId = mainPart.GetIdOfPart(imagePart);

            // Build drawing with blip referencing relId
            var element = new Drawing(
                new DW.Inline(
                    new DW.Extent { Cx = 990000L, Cy = 792000L },
                    new DW.EffectExtent { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                    new DW.DocProperties { Id = (UInt32Value)1U, Name = "Picture 1" },
                    new DW.NonVisualGraphicFrameDrawingProperties(
                        new A.GraphicFrameLocks { NoChangeAspect = true }
                    ),
                    new A.Graphic(
                        new A.GraphicData(
                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(new PIC.NonVisualDrawingProperties { Id = (UInt32Value)0U, Name = "image.png" }, new PIC.NonVisualPictureDrawingProperties()),
                                new PIC.BlipFill(
                                    new A.Blip() { Embed = relId },
                                    new A.Stretch(new A.FillRectangle())
                                ),
                                new PIC.ShapeProperties(new A.Transform2D(new A.Offset(), new A.Extents { Cx = 990000L, Cy = 792000L }))
                            )
                        ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                    )
                ) { DistanceFromTop = (UInt32Value)0U, DistanceFromBottom = (UInt32Value)0U, DistanceFromLeft = (UInt32Value)0U, DistanceFromRight = (UInt32Value)0U }
            );

            var para = new Paragraph();
            var run = new Run();
            run.Append(element);
            para.Append(run);
            body.Append(para);
            mainPart.Document.Save();
        }

        // Sanity-check: ensure the file contains the blip element before parsing
        using (var wd = WordprocessingDocument.Open(file, false))
        {
            var bodyXml = wd.MainDocumentPart.Document.Body.OuterXml;
            Assert.Contains("blip", bodyXml);
            Assert.Contains("embed", bodyXml);
            // Also verify image part is attached
            Assert.True(wd.MainDocumentPart.ImageParts.Any(), "ImagePart not found on main document part");

            var blipNode = wd.MainDocumentPart.Document.Body.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().FirstOrDefault();
            Assert.NotNull(blipNode);
            Assert.False(string.IsNullOrEmpty(blipNode?.Embed?.Value));
            var imgPart = wd.MainDocumentPart.GetPartById(blipNode!.Embed!.Value) as ImagePart;
            Assert.NotNull(imgPart);
        }

        var doc = DocxDocumentLoader.Load(file);

        var paragraph = doc.Children.OfType<DocParagraph>().FirstOrDefault();
        Assert.NotNull(paragraph);
        // Image extraction should now attach a DocImage to the corresponding run
        Assert.Contains(paragraph.Runs, r => r.Children.Any(c => c is DocImage));

        File.Delete(file);
    }

    [Fact]
    public void DocxParser_AttachesImageToCorrectRun()
    {
        var tmp = System.IO.Path.GetTempFileName();
        File.Delete(tmp);
        var file = tmp + ".docx";

        // minimal 1x1 PNG
        var pngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR4nGNgYAAAAAMAASsJTYQAAAAASUVORK5CYII=";
        var pngBytes = System.Convert.FromBase64String(pngBase64);

        using (var wordDoc = WordprocessingDocument.Create(file, WordprocessingDocumentType.Document))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new Body());
            var body = mainPart.Document.Body;

            var imagePart = mainPart.AddImagePart(ImagePartType.Png);
            using (var stream = imagePart.GetStream())
                stream.Write(pngBytes, 0, pngBytes.Length);

            var relId = mainPart.GetIdOfPart(imagePart);

            var element = new Drawing(
                new DW.Inline(
                    new DW.Extent { Cx = 990000L, Cy = 792000L },
                    new DW.EffectExtent { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                    new DW.DocProperties { Id = (UInt32Value)1U, Name = "Picture 1" },
                    new DW.NonVisualGraphicFrameDrawingProperties(
                        new A.GraphicFrameLocks { NoChangeAspect = true }
                    ),
                    new A.Graphic(
                        new A.GraphicData(
                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(new PIC.NonVisualDrawingProperties { Id = (UInt32Value)0U, Name = "image.png" }, new PIC.NonVisualPictureDrawingProperties()), new PIC.BlipFill(
                                    new A.Blip() { Embed = relId },
                                    new A.Stretch(new A.FillRectangle())
                                ),
                                new PIC.ShapeProperties(new A.Transform2D(new A.Offset(), new A.Extents { Cx = 990000L, Cy = 792000L }))
                            )
                        ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                    )
                ) { DistanceFromTop = (UInt32Value)0U, DistanceFromBottom = (UInt32Value)0U, DistanceFromLeft = (UInt32Value)0U, DistanceFromRight = (UInt32Value)0U }
            );

            var para = new Paragraph();
            var run1 = new Run(new Text("Before "));
            var run2 = new Run();
            run2.Append(element);
            var run3 = new Run(new Text(" After"));
            para.Append(run1);
            para.Append(run2);
            para.Append(run3);
            body.Append(para);
            mainPart.Document.Save();
        }

        var doc = DocxDocumentLoader.Load(file);

        var paragraph = doc.Children.OfType<DocParagraph>().FirstOrDefault();
        Assert.NotNull(paragraph);
        Assert.True(paragraph.Runs.Count >= 3);
        // the middle run should have the image attached
        Assert.Contains(paragraph.Runs[1].Children, c => c is DocImage);

        File.Delete(file);
    }

    [Fact]
    public void DocxParser_ParsesAnchorImage()
    {
        var tmp = System.IO.Path.GetTempFileName();
        File.Delete(tmp);
        var file = tmp + ".docx";

        // minimal 1x1 PNG
        var pngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR4nGNgYAAAAAMAASsJTYQAAAAASUVORK5CYII=";
        var pngBytes = System.Convert.FromBase64String(pngBase64);

        using (var wordDoc = WordprocessingDocument.Create(file, WordprocessingDocumentType.Document))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new Body());
            var body = mainPart.Document.Body;

            var imagePart = mainPart.AddImagePart(ImagePartType.Png);
            using (var stream = imagePart.GetStream())
                stream.Write(pngBytes, 0, pngBytes.Length);

            var relId = mainPart.GetIdOfPart(imagePart);

            // Build anchor drawing with blip referencing relId
            var element = new Drawing(
                new DW.Anchor(
                    new DW.SimplePosition { X = 0, Y = 0 },
                    new DW.Extent { Cx = 990000L, Cy = 792000L },
                    new DW.EffectExtent { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L },
                    new DW.DocProperties { Id = (UInt32Value)2U, Name = "Picture Anchor" },
                    new DW.NonVisualGraphicFrameDrawingProperties(
                        new A.GraphicFrameLocks { NoChangeAspect = true }
                    ),
                    new A.Graphic(
                        new A.GraphicData(
                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(new PIC.NonVisualDrawingProperties { Id = (UInt32Value)0U, Name = "image.png" }, new PIC.NonVisualPictureDrawingProperties()), new PIC.BlipFill(
                                    new A.Blip() { Embed = relId },
                                    new A.Stretch(new A.FillRectangle())
                                ),
                                new PIC.ShapeProperties(new A.Transform2D(new A.Offset(), new A.Extents { Cx = 990000L, Cy = 792000L }))
                            )
                        ) { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                    )
                ) { DistanceFromTop = (UInt32Value)0U, DistanceFromBottom = (UInt32Value)0U, DistanceFromLeft = (UInt32Value)0U, DistanceFromRight = (UInt32Value)0U }
            );

            var para = new Paragraph();
            var run1 = new Run(new Text("Start "));
            var run2 = new Run();
            run2.Append(element);
            var run3 = new Run(new Text(" End"));
            para.Append(run1);
            para.Append(run2);
            para.Append(run3);
            body.Append(para);
            mainPart.Document.Save();
        }

        var doc = DocxDocumentLoader.Load(file);

        var paragraph = doc.Children.OfType<DocParagraph>().FirstOrDefault();
        Assert.NotNull(paragraph);
        Assert.True(paragraph.Runs.Count >= 3);
        // the middle run should have the anchor image attached
        Assert.Contains(paragraph.Runs[1].Children, c => c is DocImage);

        File.Delete(file);
    }

    [Fact]
    public void Integration_ParseExampleDocxFiles()
    {
        var repoRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "..", "..", ".."));
        var inputDir = System.IO.Path.Combine(repoRoot, "examples", "docx-to-html", "input");
        Assert.True(System.IO.Directory.Exists(inputDir), $"Sample input directory not found: {inputDir}");

        var renderer = new NeoDoc.Html.Renderers.HtmlRenderer();

        foreach (var f in System.IO.Directory.GetFiles(inputDir, "*.docx"))
        {
            var doc = DocxDocumentLoader.Load(f);
            Assert.NotNull(doc);

            // ensure rendering doesn't throw and produces some output
            var html = renderer.Render(doc);
            Assert.False(string.IsNullOrWhiteSpace(html), $"HTML render was empty for {f}");
        }
    }
}
