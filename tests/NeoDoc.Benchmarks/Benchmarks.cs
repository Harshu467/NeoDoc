using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using NeoDoc.Core.Document;
using NeoDoc.Core.Nodes;
using NeoDoc.Html;
using System.IO;

namespace NeoDoc.Benchmarks;

[MemoryDiagnoser]
public class HtmlStreamerBenchmarks
{
    private DocDocument _doc;

    [Params(100, 1000)]
    public int ParagraphCount;

    [GlobalSetup]
    public void Setup()
    {
        _doc = new DocDocument();
        for (int i = 0; i < ParagraphCount; i++)
        {
            _doc.AddChild(new DocParagraph { Text = "Paragraph " + i + " - Lorem ipsum dolor sit amet." });
        }
    }

    [Benchmark]
    public void HtmlStreamer_Write_StringWriter()
    {
        using var sw = new StringWriter();
        HtmlStreamer.Write(_doc, sw);
    }

    [Benchmark]
    public void HtmlWriter_Write_MemoryStream()
    {
        using var ms = new MemoryStream();
        var writer = new NeoDoc.Html.HtmlWriter();
        using var sw = new StreamWriter(ms, leaveOpen: true);
        writer.Write(_doc, sw);
        sw.Flush();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<HtmlStreamerBenchmarks>();
    }
}
