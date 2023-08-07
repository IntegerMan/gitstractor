﻿using GitStractor.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GitStractor.Extract;

public class GitStractorExtract : GitStractorProgram {
    public override string UsageHelp => @"Usage: GitStractor-Extract -s C:\repos\YourRepo -d C:\GitStractorRaw\YourRepo";

    protected override IHostBuilder ConfigureHostBuilder(IHostBuilder builder, string[] args) =>
        builder.UseConsoleLifetime()
            .ConfigureServices((context, services) => {
                // Register dependencies
                services.AddTransient<GitDataExtractor>();

                // Detect Options
                services.AddOptions<GitStractorExtractOptions>()
                        .BindConfiguration("Extract")
                        .ValidateDataAnnotations();

                // Register our service
                services.AddHostedService<GitStractorExtractWorker>();
            })
            .ConfigureAppConfiguration(services => {
                services.AddCommandLine(args, new Dictionary<string, string>() {
                    { "-s", "Extract:SourcePath" },
                    { "--source", "Extract:SourcePath" },
                    { "-d", "Extract:OutputPath" },
                    { "--destination", "Extract:OutputPath" },
                    { "-o", "Extract:OverwriteIfExists" },
                    { "--overwrite", "Extract:OverwriteIfExists" },
                });
            });
}
