using NeoDoc.Core.Document;

namespace NeoDoc.Rules;

public static class RuleProcessor
{
    public static void Apply(DocDocument document, params IDocRule[] rules)
    {
        if (rules == null || rules.Length == 0)
            return;

        var engine = new RuleEngine(rules);
        engine.Apply(document);
    }
}
