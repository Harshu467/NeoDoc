using NeoDoc.Core.Nodes;

namespace NeoDoc.Core.Document;

public sealed class DocDocument : DocNode
{
    public IDictionary<string, string> Metadata { get; } =
        new Dictionary<string, string>();
}
