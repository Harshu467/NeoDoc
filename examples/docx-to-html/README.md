# DOCX to HTML Example

This example demonstrates how to convert a Word (DOCX) document into HTML using NeoDoc.

---

## Input

- `input/sample.docx`

The input document contains:
- Paragraph text
- A simple table
- Optional placeholders

---

## Output

- `output/sample.html`

The output HTML is generated using the NeoDoc HTML renderer.

---

## How to Run

Example code:

```csharp
using NeoDoc;

Document.Load("input/sample.docx")
        .Save("output/sample.html");
