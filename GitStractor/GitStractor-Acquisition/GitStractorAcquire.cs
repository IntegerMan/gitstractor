using GitStractor.Cloning;
using GitStractor.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GitStractor.Acquire;

public class GitStractorAcquire : GitStractorProgram {
    public override string UsageHelp => @"Usage: GitStractor-Acquire -r https://GitHub.com/IntegerMan/GitStractor -p C:\dev\GitStractor";

    protected override IHostBuilder ConfigureHostBuilder(IHostBuilder builder, string[] args) =>
        builder.UseConsoleLifetime()
            .ConfigureServices((context, services) => {
                // Dependencies needed by our worker
                services.AddTransient<RepositoryCloner>();

                // Detect Options
                services.AddOptions<GitStractorAcquireOptions>()
                        .BindConfiguration("Acquire")
                        .ValidateDataAnnotations();

                // Register our service
                services.AddHostedService<GitStractorAcquireWorker>();
            })
            .ConfigureAppConfiguration(services => {
                services.AddCommandLine(args, new Dictionary<string, string>() {
                    { "-r", "Acquire:Repository" },
                    { "--repository", "Acquire:Repository" },
                    { "-p", "Acquire:ExtractPath" },
                    { "--path", "Acquire:ExtractPath" },
                    { "-o", "Acquire:OverwriteIfExists" },
                    { "--overwrite", "Acquire:OverwriteIfExists" },
                });
            });
}
