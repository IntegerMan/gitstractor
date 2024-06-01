using GitStractor;
using GitStractor.GitObservers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Spectre.Console;

namespace GitstractorConsole.Extraction;

public class ExtractionMenu
{
    public int Run()
    {
        string path = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter the path to the Git repository")
                .PromptStyle("green")
                .DefaultValue("/home/matteland/Documents/EmergenceWin")
        );

        string? gitRepo = GitStractor.Utilities.FileUtilities.GetParentGitDirectory(path);

        if (string.IsNullOrWhiteSpace(gitRepo))
        {
            AnsiConsole.MarkupLine("[red]No Git repository found in the specified path.[/]");
            return 1;
        }

        AnsiConsole.MarkupLine($"Extracting data from git repo in [yellow]{gitRepo}[/]");
        AnsiConsole.WriteLine();

        string outputPath = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter the output path")
                .PromptStyle("green")
                .DefaultValue("/home/matteland/data/emergence")
        );

        // Combine the output path with the current directory to get the full path.
        outputPath = Path.Combine(Environment.CurrentDirectory, outputPath);

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

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLineInterpolated($"[bold green]Data extraction complete to[/] [bold yellow]{outputPath}[/]!");

        return 0;
    }
}