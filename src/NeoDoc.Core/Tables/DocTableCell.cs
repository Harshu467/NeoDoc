using NeoDoc.Core.Nodes;

namespace NeoDoc.Core.Tables;

public sealed class DocTableCell : DocNode
{
    public int ColSpan { get; set; } = 1;
    public int RowSpan { get; set; } = 1;
}
