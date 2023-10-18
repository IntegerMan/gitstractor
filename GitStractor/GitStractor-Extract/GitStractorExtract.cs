using GitStractor.GitObservers;
using GitStractor.Workers;

namespace GitStractor.Extract;

public class GitStractorExtract : GitStractorProgram {
    public override string UsageHelp => @"Usage: GitStractor-Extract -s C:\repos\YourRepo -d C:\GitStractorRaw\YourRepo";

    protected override IHostBuilder ConfigureHostBuilder(IHostBuilder builder, string[] args) =>
        builder.UseConsoleLifetime()
            .ConfigureServices((context, services) => {
                // Register dependencies
                services.AddTransient<GitTreeWalker>();
                services.AddTransient<IGitObserver, LoggingGitObserver>();
                services.AddTransient<IGitObserver, SummaryAuthorObserver>();
                services.AddTransient<IGitObserver, AuthorYearlyCommitObserver>();
                services.AddTransient<IGitObserver, AuthorQuarterlyCommitObserver>();
                services.AddTransient<IGitObserver, AuthorMonthlyCommitObserver>();
                services.AddTransient<IGitObserver, AuthorWeeklyCommitObserver>();
                services.AddTransient<IGitObserver, AuthorDailyCommitObserver>();
                services.AddTransient<IGitObserver, GitCommitObserver>();
                services.AddTransient<IGitObserver, CommitWorkItemObserver>();
                services.AddTransient<IGitObserver, FileObserver>();
                services.AddTransient<IGitObserver, DenormalizedFileCommitObserver>();
                services.AddTransient(provider => provider.GetServices<IGitObserver>().ToList());

                services.AddTransient<GitDataExtractor>();

                // Detect Options
                services.AddOptions<GitStractorExtractOptions>()
                        .BindConfiguration("Extract")
                        .ValidateDataAnnotations();

                // Register our service
                services.AddHostedService<GitStractorExtractWorker>();
            })
            .ConfigureAppConfiguration(services => {
                services.AddCommandLine(args, new Dictionary<string, string> {
                    { "-s", "Extract:SourcePath" },
                    { "--source", "Extract:SourcePath" },
                    { "-d", "Extract:OutputPath" },
                    { "--destination", "Extract:OutputPath" },
                    { "-a", "Extract:AuthorMapPath" },
                    { "--authormap", "Extract:AuthorMapPath" },
                    { "-b", "Extract:IncludeBranchDetails" },
                    { "--includebranches", "Extract:IncludeBranchDetails" },
                    { "-i", "Extract:IgnorePatterns" },
                    { "--ignore", "Extract:IgnorePatterns" },
                });
            });
}

