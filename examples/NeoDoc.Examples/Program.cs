using NeoDoc;
using NeoDoc.Rules.Rules;

Console.WriteLine("Running NeoDoc examples...");

// Get repo root reliably
var appBase = AppContext.BaseDirectory;

// Navigate up to repo root
var repoRoot = Path.GetFullPath(Path.Combine(appBase, "..", "..", "..", "..", ".."));

// Example paths
var exampleRoot = Path.Combine(repoRoot, "examples", "docx-to-html");
var inputPath = Path.Combine(exampleRoot, "input", "sample.docx");
var outputPath = Path.Combine(exampleRoot, "output", "sample.html");

var data = new Dictionary<string, string>
{
    ["CustomerName"] = "Acme Corp",
    ["InvoiceNumber"] = "INV-1001"
};

try
{
        Document.Load(inputPath)
            .ApplyRules(new PlaceholderRule(data))
            .SaveStreamed(outputPath);

    Console.WriteLine("DOCX to HTML example completed.");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("NeoDoc error:");
    Console.WriteLine(ex.Message);
    Console.ResetColor();
}
