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
                services.AddTransient<RepositoryCloner>();

                services.AddOptions<GitStractorAcquireOptions>()
                        .Configure(options => context.Configuration.GetSection("Acquire").Bind(options))
                        .ValidateDataAnnotations();

                // Register our service
                services.AddHostedService<GitStractorAcquisitionWorker>();
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
