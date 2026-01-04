using NeoDoc.Core.Document;

namespace NeoDoc.Rules;

public interface IDocRule
{
    void Apply(DocDocument document);
}
