using NeoDoc.Core.Nodes;

namespace NeoDoc.Core.Tables;

public sealed class DocTableRow : DocNode
{
    public IList<DocTableCell> Cells { get; } = new List<DocTableCell>();
}
