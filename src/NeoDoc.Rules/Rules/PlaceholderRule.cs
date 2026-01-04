using System.Text.RegularExpressions;
using NeoDoc.Core.Document;
using NeoDoc.Core.Nodes;

namespace NeoDoc.Rules.Rules;

public sealed class PlaceholderRule : IDocRule
{
    private readonly IReadOnlyDictionary<string, string> _values;

    private static readonly Regex PlaceholderRegex =
        new(@"\{(?<key>[^\}]+)\}", RegexOptions.Compiled);

    public PlaceholderRule(IDictionary<string, string> values)
    {
        _values = new Dictionary<string, string>(values);
    }

    public void Apply(DocDocument document)
    {
        Traverse(document);
    }

    private void Traverse(DocNode node)
    {
        if (node is DocParagraph paragraph)
        {
            paragraph.Text = ReplacePlaceholders(paragraph.Text);
        }

        foreach (var child in node.Children)
        {
            Traverse(child);
        }
    }

    private string ReplacePlaceholders(string text)
    {
        return PlaceholderRegex.Replace(text, match =>
        {
            var key = match.Groups["key"].Value;

            return _values.TryGetValue(key, out var value)
                ? value
                : match.Value; // keep placeholder if not found
        });
    }
}
