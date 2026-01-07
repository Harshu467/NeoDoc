namespace NeoDoc.Core.Nodes;

public sealed class DocImage : DocNode
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
    public string? Name { get; set; }
}