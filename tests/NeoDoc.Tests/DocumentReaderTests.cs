using System;
using System.IO;
using System.Linq;
using NeoDoc;
using NeoDoc.Core.Document;
using NeoDoc.Core.Nodes;
using NeoDoc.Docx;
using Xunit;

namespace NeoDoc.Tests;

public class DocumentReaderTests
{
    [Fact]
    public void DocxDocumentReader_StreamsNodesFromSampleDocx()
    {
        // Locate repo root from test assembly location
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var inputPath = Path.Combine(repoRoot, "examples", "docx-to-html", "input", "sample.docx");

        Assert.True(File.Exists(inputPath), $"Test DOCX not found: {inputPath}");

        var reader = new DocxDocumentReader(inputPath);
        var nodes = reader.ReadNodes().ToList();

        Assert.NotEmpty(nodes);
        Assert.Contains(nodes, n => n is DocParagraph || n is DocNode);
    }
}
