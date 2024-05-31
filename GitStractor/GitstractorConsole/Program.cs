using System.Text;
using GitStractor;
using GitStractor.GitObservers;
using GitstractorConsole;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Spectre.Console;

try
{
    // Using UTF8 allows more capabilities for Spectre.Console.
    Console.OutputEncoding = Encoding.UTF8;
    Console.InputEncoding = Encoding.UTF8;

    AnsiConsole.Write(new FigletText("GitStractor").Color(Color.Aqua));

    AnsiConsole.MarkupLine(
        $"[bold yellow]GitStractor[/] is a tool to extract data from Git repositories.{Environment.NewLine}");

    string path = AnsiConsole.Prompt(
        new TextPrompt<string>("Enter the path to the Git repository")
            .PromptStyle("green")
            .DefaultValue(".")
    );
    AnsiConsole.WriteLine();

    string? gitRepo = GitStractor.Utilities.FileUtilities.GetParentGitDirectory(path);

    if (string.IsNullOrWhiteSpace(gitRepo))
    {
        AnsiConsole.MarkupLine("[red]No Git repository found in the specified path.[/]");
        return 1;
    }

    AnsiConsole.MarkupLine($"Extracting data from git repo in [yellow]{gitRepo}[/]");

    string outputPath = "extracted-output";
    AnsiConsole.MarkupLine($"Extracting data to [yellow]{outputPath}[/]");

    AnsiConsole.Progress().Start(context =>
    {
        List<IGitObserver> observers = new()
        {
            new AnsiConsoleProgressLogger(context),
            new SummaryAuthorObserver(),
            new AuthorYearlyCommitObserver(),
            new AuthorQuarterlyCommitObserver(),
            new AuthorMonthlyCommitObserver(),
            new AuthorWeeklyCommitObserver(),
            new AuthorDailyCommitObserver(),
            new GitCommitObserver(),
            new CommitWorkItemObserver(),
            new FileObserver(),
            new DenormalizedFileCommitObserver()
        };

        ILogger<GitTreeWalker> treeLogger = new NullLogger<GitTreeWalker>();
        ILogger<GitDataExtractor> extractLogger = new NullLogger<GitDataExtractor>();
        
        GitTreeWalker walker = new(treeLogger);
        GitDataExtractor extractor = new(extractLogger, observers, walker);
        
        extractor.ExtractInformation(gitRepo,
            outputPath: outputPath,
            authorMapPath: null,
            includeBranchDetails: false,
            ignorePatterns: []);
    });

    return 0;
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
    return 1;
}