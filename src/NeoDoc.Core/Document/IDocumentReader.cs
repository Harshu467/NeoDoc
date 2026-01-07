using System.Collections.Generic;
using NeoDoc.Core.Nodes;

namespace NeoDoc.Core.Document;

public interface IDocumentReader
{
    /// <summary>
    /// Reads top-level document nodes (paragraphs, tables) in a streaming fashion.
    /// Enumeration keeps any underlying resources open until the enumerator is disposed.
    /// </summary>
    IEnumerable<DocNode> ReadNodes();
}
