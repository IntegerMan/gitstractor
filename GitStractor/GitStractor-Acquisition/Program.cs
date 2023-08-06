using GitStractor.Cloning;
using GitStractor.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GitStractor.Acquire;

public class Program {

    public static int Main(string[] args) {
        IHostBuilder builder = CreateHostBuilder(args);

        try {
            using GitStractorHost host = (GitStractorHost)builder.Build();
            host.Run();

            return host.Succeeded ? 0 : -1;
        }
        catch (OptionsValidationException ex) {
            // Validate the provided configuration options
            WriteErrorToConsole(ex.Message);
            WriteHelpToConsole(@"Usage: GitStractor-Acquire -r https://GitHub.com/IntegerMan/GitStractor -p C:\dev\GitStractor");
            return -2;
        }
        catch (AggregateException ex) {
            // This typically happens when a worker encounters an exception shutting down
            WriteErrorToConsole(ex.Message);

            return -3;
        }
    }

    private static void WriteErrorToConsole(string message) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine();
        Console.Error.WriteLine(message);
        Console.WriteLine();
        Console.ResetColor();
    }

    private static void WriteHelpToConsole(string message) {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(message);
        Console.ResetColor();
    }


    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseConsoleLifetime()
            .ConfigureHostOptions(hostOptions => {
                hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost;
                hostOptions.ShutdownTimeout = TimeSpan.FromSeconds(30);
            })
            .ConfigureServices((hostContext, services) => {
                services.AddSingleton<IHost, GitStractorHost>();
                services.AddOptions<GitStractorAcquireOptions>()
                        .Configure(options => {
                            // Use our environment variable and config file to specify defaults
                            hostContext.Configuration.GetSection("Acquire").Bind(options);
                        })
                        .ValidateDataAnnotations();
                services.AddTransient<RepositoryCloner>();

                // Register our service
                services.AddHostedService<GitStractorAcquisitionWorker>();
            })
            .ConfigureAppConfiguration(services => {
                services.AddEnvironmentVariables(prefix: "GITSTRACTOR_");
                services.AddCommandLine(args, new Dictionary<string, string>() {
                    { "-r", "Acquire:Repository" },
                    { "--repository", "Acquire:Repository" },
                    { "-p", "Acquire:ExtractPath" },
                    { "--path", "Acquire:ExtractPath" },
                    { "-o", "Acquire:OverwriteIfExists" },
                    { "--overwrite", "Acquire:OverwriteIfExists" },
                });
            })
            .ConfigureLogging((hostContext, config) => {
                config.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                config.AddConsole();
            });
}