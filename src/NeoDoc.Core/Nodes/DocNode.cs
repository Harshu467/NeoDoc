namespace NeoDoc.Core.Nodes;

public abstract class DocNode
{
    public DocNode? Parent { get; internal set; }

    public IList<DocNode> Children { get; } = new List<DocNode>();

    public void AddChild(DocNode node)
    {
        node.Parent = this;
        Children.Add(node);
    }
}
