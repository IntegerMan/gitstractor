using System.Diagnostics;
using GitStractor;
using GitStractor.GitObservers;
using GitStractor.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.ML;
using Spectre.Console;

namespace GitstractorConsole.Extraction;

public class ExtractionMenu
{
    public int Run()
    {
        string path = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter the path to the Git repository")
                .PromptStyle("green")
                .DefaultValue("/home/matteland/Documents/machinelearning")
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
                .DefaultValue("/home/matteland/data/GitStractorOutput")
        );

        // Combine the output path with the current directory to get the full path.
        outputPath = Path.Combine(Environment.CurrentDirectory, outputPath);

        AnsiConsole.MarkupLine($"Extracting data to [yellow]{outputPath}[/]");

        ServiceCollection services = new();
        services.AddPredictionEnginePool<CommitClassifierInput, CommitClassification>()
            .FromFile("BugfixClassifier.zip", watchForChanges: false);
        
        services.AddScoped<GitCommitWithBugfixClassificationObserver>();
        services.AddScoped<DenormalizedFileCommitWithBugFixClassificationObserver>();
        
        IServiceProvider provider = services.BuildServiceProvider();

        Stopwatch sw = new();
        AnsiConsole.Progress().Start(context =>
        {
            List<IGitObserver> observers =
            [
                new AnsiConsoleProgressLogger(context),
                new SummaryAuthorObserver(),
                new AuthorYearlyCommitObserver(),
                new AuthorQuarterlyCommitObserver(),
                new AuthorMonthlyCommitObserver(),
                new AuthorWeeklyCommitObserver(),
                new AuthorDailyCommitObserver(),
                provider.GetRequiredService<GitCommitWithBugfixClassificationObserver>(),
                new CommitWorkItemObserver(),
                new FileObserver(),
                provider.GetRequiredService<DenormalizedFileCommitWithBugFixClassificationObserver>()
            ]; // /home/matteland/Documents/Wherewolf, /home/matteland/data/wherewolf

            ILogger<GitTreeWalker> treeLogger = new NullLogger<GitTreeWalker>();
            ILogger<GitDataExtractor> extractLogger = new NullLogger<GitDataExtractor>();

            GitTreeWalker walker = new(treeLogger);
            GitDataExtractor extractor = new(extractLogger, observers, walker);

            sw = Stopwatch.StartNew();
            extractor.ExtractInformation(gitRepo,
                outputPath: outputPath,
                authorMapPath: null,
                includeBranchDetails: false,
                ignorePatterns: []);
            sw.Stop();
        });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLineInterpolated($"[bold green]Data extraction complete to[/] [bold yellow]{outputPath}[/] in [bold yellow]{sw.Elapsed.TotalSeconds:F2}[/] seconds");

        return 0;
    }
}