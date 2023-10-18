using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GitStractor.Workers;

public class GitStractorHost : IHost {
    private int _runningServices;
    private readonly IHostLifetime _hostLifetime;
    private readonly ApplicationLifetime _applicationLifetime;
    private readonly HostOptions _options;
    private readonly GitStractorHostOptions _gitStractorOptions;
    private IEnumerable<IHostedService>? _hostedServices;

    private bool _started;
    private bool _stopped;
    private bool _errorsEncountered;

    public static string Name => "GitStractor Host";

    private static readonly Action<ILogger, Exception?> logStarting = LoggerMessage.Define(LogLevel.Debug, new EventId(1, "StartingHost"), $"Starting {Name}");
    private static readonly Action<ILogger, Exception?> logStarted = LoggerMessage.Define(LogLevel.Debug, new EventId(2, "StartedHost"), $"Started {Name}");
    private static readonly Action<ILogger, Exception?> logStopping = LoggerMessage.Define(LogLevel.Debug, new EventId(3, "StoppingHost"), $"Stopping {Name}");
    private static readonly Action<ILogger, Exception?> logStopped = LoggerMessage.Define(LogLevel.Debug, new EventId(4, "StoppedHost"), $"Stopped {Name}");
    private static readonly Action<ILogger, Exception?> logStoppedWithExceptions = LoggerMessage.Define(LogLevel.Error, new EventId(5, "StoppedHost"), $"Stopped {Name} but encountered one or more exceptions stopping workers");
    private static readonly Action<ILogger, string, Exception?> logServiceRegistered = LoggerMessage.Define<string>(LogLevel.Debug, new EventId(6, "ServiceRegistered"), "Host notified that {ServiceName} will complete when work is finished");
    private static readonly Action<ILogger, string, Exception?> logServiceStoppedEarly = LoggerMessage.Define<string>(LogLevel.Information, new EventId(6, "ServiceStoppedEarly"), "Host notified that {ServiceName} completed its work");
    private static readonly Action<ILogger, string, Exception?> logServiceStoppedEarlyDueToFailure = LoggerMessage.Define<string>(LogLevel.Error, new EventId(7, "ServiceStoppedEarly"), "Host notified that {ServiceName} encountered an error and could not complete its work");
    private static readonly Action<ILogger, Exception?> logStoppingEarly = LoggerMessage.Define(LogLevel.Information, new EventId(8, "StoppingHostEarly"), $"All workers completed. {Name} will now shut down.");
    private static readonly Action<ILogger, Exception?> logNoServices = LoggerMessage.Define(LogLevel.Warning, new EventId(4, "NoServices"), $"{Name} started without any services registered.");

    public ILogger<GitStractorHost> Log { get; }

    public GitStractorHost(IServiceProvider services, IHostApplicationLifetime applicationLifetime, ILogger<GitStractorHost> logger,
        IHostLifetime hostLifetime, IOptions<HostOptions> options, IOptions<GitStractorHostOptions> gitstractorOptions) {

        Services = services;
        Log = logger;
        _applicationLifetime = (ApplicationLifetime)applicationLifetime;
        _hostLifetime = hostLifetime;
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _gitStractorOptions = gitstractorOptions.Value;
    }

    public IServiceProvider Services { get; }

    public async Task StartAsync(CancellationToken cancellationToken = default) {
        // Start up the host
        logStarting(Log, null);
        await _hostLifetime.WaitForStartAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        // Start up the services
        _hostedServices = Services.GetService<IEnumerable<IHostedService>>();
        if (_hostedServices != null) {
            foreach (IHostedService hostedService in _hostedServices) {
                // I want some services to be able to complete early. If all services complete early, the host should be able to complete early as well
                if (hostedService is ICanStopEarly earlyStopService) {
                    logServiceRegistered(Log, earlyStopService.Name, null);
                    earlyStopService.InvokeOnCompleted(WorkerCompleted);
                }

                // Start the service
                await hostedService.StartAsync(cancellationToken).ConfigureAwait(false);

                // Register that our service is running - this lets us know when we can close the host later
                Interlocked.Increment(ref _runningServices);
            }
        }

        // Once we're up and running, let others know
        _started = true;
        _applicationLifetime.NotifyStarted();
        logStarted(Log, null);

        // Handle the case where no services were registered.
        // This should probably only happen during dev or if things get very adventurous in configuration
        if (_runningServices == 0) {
            logNoServices(Log, null);

            if (_gitStractorOptions.AllowEarlyStop) {
                PerformEarlyStop();
            }
        }
    }

    private void WorkerCompleted(ICanStopEarly service, bool succeeded) {
        if (succeeded) {
            logServiceStoppedEarly(Log, service.Name, null);
        } else {
            logServiceStoppedEarlyDueToFailure(Log, service.Name, null);
            _errorsEncountered = true;
        }

        int remainingServices = Interlocked.Decrement(ref _runningServices);

        if (remainingServices <= 0 && _gitStractorOptions.AllowEarlyStop) {
            PerformEarlyStop();
        }
    }

    private void PerformEarlyStop() {
        logStoppingEarly(Log, null);
        _applicationLifetime.StopApplication();
    }

    public async Task StopAsync(CancellationToken cancellationToken = default) {
        logStopping(Log, null);

        using (var cts = new CancellationTokenSource(_options.ShutdownTimeout))
        using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken)) {
            CancellationToken token = linkedCts.Token;
            _applicationLifetime.StopApplication();

            List<Exception> exceptions = new();
            if (_hostedServices != null) {
                foreach (var hostedService in _hostedServices.Reverse()) {
                    token.ThrowIfCancellationRequested();
                    try {
                        await hostedService.StopAsync(token).ConfigureAwait(false);
                    }
                    catch (Exception ex) {
                        exceptions.Add(ex);
                        _errorsEncountered = true;
                    }
                }
            }

            token.ThrowIfCancellationRequested();
            await _hostLifetime.StopAsync(token);

            _applicationLifetime.NotifyStopped();

            if (exceptions.Count > 0) {
                AggregateException ex = new("One or more hosted services failed to stop.", exceptions);
                logStoppedWithExceptions(Log, ex);
                throw ex;
            }
        }

        logStopped(Log, null);

        _stopped = true;
    }

    public void Dispose() {
        (Services as IDisposable)?.Dispose();
        GC.SuppressFinalize(this);
    }

    public bool Succeeded => _started && _stopped && !_errorsEncountered;
}
