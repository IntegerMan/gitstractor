using CommandLine;
using GitStractor.Cloning;
using GitStractor.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GitStractor.Acquire;

public class Program {

    public static int Main(string[] args) {
        IHostBuilder builder = CreateHostBuilder(args);

        try {
            using GitStractorHost host = (GitStractorHost)builder.Build();
            host.Run();

            return host.Succeeded ? 0 : -1;
        }
        catch (ConfigurationException ex) {
            // This happens when we can't create the host builder
            Console.Error.WriteLine(ex.Message);

            return -2;
        }
        catch (AggregateException ex) {
            // This typically happens when a worker encounters an exception shutting down
            Console.Error.WriteLine(ex.Message);

            return -3;
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseConsoleLifetime()
            .ConfigureHostOptions(hostOptions => {
                hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost;
                hostOptions.ShutdownTimeout = TimeSpan.FromSeconds(5);
            })
            .ConfigureServices((hostContext, services) => {
                services.AddSingleton<IHost, GitStractorHost>();
                services.AddSingleton(serviceProvider => LoadOptions(args, hostContext));
                services.AddTransient<RepositoryCloner>();

                // Register our service
                services.AddHostedService<GitStractorAcquisitionWorker>();
            })
            .ConfigureAppConfiguration(services => {
                services.AddEnvironmentVariables(prefix: "GITSTRACTOR_");
            })
            .ConfigureLogging((hostContext, config) => {
                config.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                config.AddConsole();
            });

    private static GitStractorAcquireOptions LoadOptions(string[] args, HostBuilderContext hostContext) {
        GitStractorAcquireOptions options = new();
        // Use our environment variable and config file to specify defaults
        hostContext.Configuration.GetSection("Acquire").Bind(options);

        if (args.Length > 0) {
            // TODO: At this point CommandLine is hurting more than it helps
            Parser parser = new(config => {
                config.AllowMultiInstance = false;
                config.AutoVersion = true;
                config.AutoHelp = true;
                config.CaseInsensitiveEnumValues = true;
                config.CaseSensitive = false;
                config.HelpWriter = Console.Error;
            });
            ParserResult<GitStractorAcquireOptions> parserResult =
               parser.ParseArguments<GitStractorAcquireOptions>(args);

            // If this passes validation, apply the resulting values to our configured object
            if (parserResult.Value != null) {
                options.OverwriteIfExists = options.OverwriteIfExists || parserResult.Value.OverwriteIfExists;
                options.ExtractPath = parserResult.Value.ExtractPath ?? options.ExtractPath;
                options.Repository = parserResult.Value.Repository ?? options.Repository;
            }
        }

        if (!options.IsValid) {
            throw new ConfigurationException("Required configuration options were not supplied");
        }

        return options;
    }
}