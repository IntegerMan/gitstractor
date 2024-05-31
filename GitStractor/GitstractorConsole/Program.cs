using System.Text;
using GitstractorConsole.Classification;
using GitstractorConsole.Extraction;
using Spectre.Console;

try
{
    // Using UTF8 allows more capabilities for Spectre.Console.
    Console.OutputEncoding = Encoding.UTF8;
    Console.InputEncoding = Encoding.UTF8;

    AnsiConsole.Write(new FigletText("GitStractor").Color(Color.Aqua));
    AnsiConsole.MarkupLine($"[bold yellow]GitStractor[/] is a tool to extract data from Git repositories.{Environment.NewLine}");

    Dictionary<string, Func<int>> choices = new()
    {
        {
            "Extract data from a Git repository", () =>
            {
                ExtractionMenu extraction = new();
                return extraction.Run();
            }
        },
        {
            "Classify commits", () =>
            {
                CommitClassification classification = new();
                return classification.Run();
            }
        },
        {
            "Exit", () =>
            {
                AnsiConsole.WriteLine("Thank you for using [Yellow]GitStractor[/]");
                return 0;
            }
        }
    };

    return choices[AnsiConsole.Prompt(new SelectionPrompt<string>()
        .Title("Select a task")
        .AddChoices(choices.Keys))]();
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
    return 1;
}