using NeoDoc.Core.Nodes;

namespace NeoDoc.Core.Tables;

public sealed class DocTable : DocNode
{
    public IList<DocTableRow> Rows { get; } = new List<DocTableRow>();
}
