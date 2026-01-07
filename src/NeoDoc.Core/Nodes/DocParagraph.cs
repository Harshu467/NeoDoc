using System.Linq;

namespace NeoDoc.Core.Nodes;

public sealed class DocParagraph : DocNode
{
    // Legacy convenience property; kept for compatibility
    public string Text { get; set; } = string.Empty;

    // New inline runs (supports formatting and inline images via DocRun and DocImage)
    public IList<DocRun> Runs { get; } = new List<DocRun>();

    public void UpdateTextFromRuns()
    {
        Text = string.Join(string.Empty, Runs.Select(r => r.Text));
    }
}
