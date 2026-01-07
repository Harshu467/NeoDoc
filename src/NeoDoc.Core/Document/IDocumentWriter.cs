using System.IO;
using NeoDoc.Core.Nodes;

namespace NeoDoc.Core.Document;

public interface IDocumentWriter
{
    /// <summary>
    /// Writes the provided document to the given <see cref="TextWriter"/>.
    /// Implementations should stream output and avoid producing a single large string.
    /// </summary>
    void Write(DocDocument document, TextWriter writer);
}
