using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GitStractor.Acquire;

public class Program {

    public static void Main(string[] args) {
        IHostBuilder builder = Host
             .CreateDefaultBuilder(args)
             .UseConsoleLifetime()
             .ConfigureHostOptions(hostOptions => {
                 hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost;
                 hostOptions.ShutdownTimeout = TimeSpan.FromSeconds(5);
             })
             .ConfigureHostConfiguration(config => {
                 config.AddEnvironmentVariables();
                 config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                 if (args != null) {
                     config.AddCommandLine(args);
                 }
             })
             .ConfigureAppConfiguration(config => {
                 config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                 config.AddEnvironmentVariables();

                 if (args != null) {
                     config.AddCommandLine(args);
                 }
             })
             .ConfigureServices((hostContext, services) => {
                 services.AddSingleton<IHost, GitStractorHost>();
                 services.AddScoped(serviceProvider => LoadOptions(args, hostContext));
                 services.AddTransient<RepositoryCloner>();

                 // Register our service
                 services.AddHostedService<GitStractorAcquisitionWorker>();
             })
             .ConfigureLogging((hostContext, config) => {
                 config.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                 config.AddConsole();
             });

        try {
            using IHost host = builder.Build();
            host.Run();
        }
        catch (ConfigurationException ex) {
            Console.WriteLine(ex.Message);
        }
    }

    private static GitStractorAcquireOptions LoadOptions(string[] args, HostBuilderContext hostContext) {
        GitStractorAcquireOptions options = new();
        // Use our environment variable and config file to specify defaults
        hostContext.Configuration.GetSection("Acquire").Bind(options);

        Parser parser = new(config => {
            config.AllowMultiInstance = false;
            config.AutoVersion = true;
            config.AutoHelp = true;
            config.CaseInsensitiveEnumValues = true;
            config.CaseSensitive = false;
            config.HelpWriter = Console.Error;
        });
        ParserResult<GitStractorAcquireOptions> parserResult =
           parser.ParseArguments(() => options, args);

        // If this passes validation, grab the resulting values
        if (parserResult.Value != null) {
            options = parserResult.Value;
        }

        if (!options.IsValid) {
            throw new ConfigurationException("Required configuration options were not supplied", parserResult.Errors);
        }

        return options;
    }
}