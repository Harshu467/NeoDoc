# NeoDoc

NeoDoc is a lightweight document conversion library focused on streaming-friendly APIs and modular format handlers.

Quick build:

```bash
dotnet build NeoDoc.slnx -c Release
```

Example (DOCX -> HTML):

```bash
dotnet run --project examples/NeoDoc.Examples/NeoDoc.Examples.csproj
```

Repository layout:
- `src/NeoDoc.Core` — core document model
- `src/NeoDoc.Docx` — DOCX parser
- `src/NeoDoc.Html` — HTML renderer and streamer
- `src/NeoDoc.Rules` — transformation rules
- `examples/NeoDoc.Examples` — example application
Understood.
Below is a **clean, professional, internal-facing `README.md`** suitable for a **commercial product repository** that your **team members** will see.
No marketing fluff, no open-source tone — this is how real SDK teams write internal READMEs.

You can **copy–paste this directly**.

---

```markdown
# NeoDoc

NeoDoc is a document processing engine that converts structured **Word (DOCX) documents into clean, semantic HTML**.

The project is designed for **template-driven documents, reports, and forms**, where structure, rules, and consistency are more important than pixel-perfect rendering.

NeoDoc is a **commercial SDK** developed by NeoForge Labs.

---

## Project Goals

- Provide a stable, extensible DOCX → HTML engine
- Focus on document structure rather than visual fidelity
- Support complex tables, bookmarks, and rule-based processing
- Maintain a clean, modular architecture suitable for long-term evolution
- Enable future commercial licensing and subscriptions

---

## Non-Goals

The following are intentionally **out of scope** for the initial versions:

- Pixel-perfect Word rendering
- PDF generation (planned later via HTML)
- Excel or PowerPoint processing
- Visual template or designer tools

---

## Architecture Overview

NeoDoc follows a layered architecture with a strict separation of concerns.

```

NeoDoc (Public API)
├── NeoDoc.Core     - Format-agnostic document model
├── NeoDoc.Docx     - DOCX parsing (OpenXML-based)
├── NeoDoc.Rules    - Rules and conditional processing
└── NeoDoc.Html     - HTML rendering

```

**Architecture principles:**
- No direct format-to-format conversion
- All processing goes through the Core document model
- Public API is minimal and stable
- Internal components are replaceable and testable

---

## Solution Structure

```

src/
├── NeoDoc.Core
├── NeoDoc.Docx
├── NeoDoc.Html
├── NeoDoc.Rules
└── NeoDoc          // Public API facade

tests/
└── (unit and integration tests)

````

---

## Public API (Current)

```csharp
using NeoDoc;

var document = Document.Load("input.docx");

document.ApplyRules();

document.Save("output.html");
````

The public API is intentionally small.
Most implementation details remain internal to allow safe refactoring.

---

## Development Guidelines

* `NeoDoc.Core` must remain format-agnostic
* No HTML or OpenXML references in Core
* Parsers and renderers must not depend on each other
* Public APIs must remain backward-compatible once released
* Favor clarity and maintainability over premature optimization

---

## Build

The solution targets the .NET SDK.

```bash
dotnet build
```

All projects should build without warnings or errors.

---

## Status

* Core document model: implemented
* Project structure: finalized
* DOCX parser: in progress
* HTML renderer: in progress
* Rules engine: foundation in place
* Licensing system: planned

---

## Licensing & Distribution

NeoDoc is proprietary software.

This repository does not grant any rights for redistribution or commercial use.
Licensing, subscriptions, and usage terms will be defined separately.

---

## Ownership

NeoDoc is owned and maintained by **NeoForge Labs**.

All contributions must align with the project's architectural and commercial goals.

```

---

### ✅ This README is:
- Internal-team friendly
- Commercially safe
- Professional
- Architecture-focused
- Ready for long-term development


```
