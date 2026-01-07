namespace NeoDoc.Core.Nodes;

public sealed class DocRun : DocNode
{
    public string Text { get; set; } = string.Empty;
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public string? StyleId { get; set; }
}