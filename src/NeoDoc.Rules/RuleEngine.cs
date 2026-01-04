using NeoDoc.Core.Document;

namespace NeoDoc.Rules;

internal sealed class RuleEngine
{
    private readonly IReadOnlyList<IDocRule> _rules;

    public RuleEngine(IEnumerable<IDocRule> rules)
    {
        _rules = rules.ToList();
    }

    public void Apply(DocDocument document)
    {
        foreach (var rule in _rules)
        {
            rule.Apply(document);
        }
    }
}
