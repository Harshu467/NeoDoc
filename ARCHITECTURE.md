# NeoDoc Architecture

High-level architecture summary for NeoDoc (initial MVP).

Core ideas:
- `NeoDoc.Core` — document model (`DocDocument`, `DocNode`, paragraphs, tables).
- `NeoDoc.Docx` — DOCX parsing layer using Open XML SDK (synchronous parser `DocxParser`).
- `NeoDoc.Html` — HTML rendering, both in-memory (`HtmlRenderer`) and streaming (`HtmlStreamer`).
- `NeoDoc.Rules` — rule engine for document transformations.

Design principles:
- Streaming-first where possible to reduce memory usage.
- Small, testable modules with clear responsibilities.
- Pluggable format handlers and renderers for future formats.

Next extension points:
- `IDocumentReader` / `IDocumentWriter` abstractions for incremental parsing.
- Native rendering backends (Skia, HarfBuzz) for advanced layout.
