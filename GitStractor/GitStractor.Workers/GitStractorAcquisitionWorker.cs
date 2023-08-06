using GitStractor.Cloning;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GitStractor.Workers; 

public class GitStractorAcquisitionWorker : IHostedService, ICanStopEarly, IDisposable {
    private readonly GitStractorAcquireOptions options;
    private readonly RepositoryCloner cloner;
    private readonly List<Action<ICanStopEarly, bool>> _invokeOnCompleted = new();

    private static readonly Action<ILogger, Exception?> logStarted = LoggerMessage.Define(LogLevel.Information, new EventId(1, "StartingClone"), "Registering Clone Task");
    private static readonly Action<ILogger, Exception?> logExecuting = LoggerMessage.Define(LogLevel.Information, new EventId(1, "RunningClone"), "Running Clone Task");
    private static readonly Action<ILogger, Exception?> logStopping = LoggerMessage.Define(LogLevel.Information, new EventId(1, "StoppingClone"), "Stopping Clone Task");
    private static readonly Action<ILogger, string, Exception?> logWorkError = LoggerMessage.Define<string>(LogLevel.Error, new EventId(1, "CloneError"), "Error running clone task: {ErrorMessage}");
    private static readonly Action<ILogger, string, Exception?> logCloned = LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, "Cloned"), "Repository cloned to {Path}");

    private Timer? _timer;
    private CancellationToken _token;
    private Task _task = Task.CompletedTask;
    private bool disposedValue;

    public ILogger<GitStractorAcquisitionWorker> Log { get; }

    public string Name => "GitStractor-Acquire";

    public const int InitialDelayInSeconds = 2;

    // TODO: Use an IOptions<GitStractorAcquireOptions> here
    public GitStractorAcquisitionWorker(ILogger<GitStractorAcquisitionWorker> log, GitStractorAcquireOptions options, RepositoryCloner cloner) {
        Log = log;
        this.options = options;
        this.cloner = cloner;
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        logStarted(Log, null);
        _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(InitialDelayInSeconds), TimeSpan.FromMilliseconds(-1));
        _token = cancellationToken;

        return _task;
    }

    private void DoWork(object? state) {
        bool succeeded = true;

        try {
            logExecuting(Log, null);

            string finalPath = cloner.Clone(options.Repository, options.ExtractPath, options.OverwriteIfExists);

            logCloned(Log, finalPath, null);
        }
        catch (CloneException ex) {
            logWorkError(Log, ex.Message, ex);
            succeeded = false;
        }

        NotifyWorkCompleted(succeeded);
    }

    private void NotifyWorkCompleted(bool succeeded) {
        foreach (var listener in _invokeOnCompleted) {
            listener.Invoke(this, succeeded);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        logStopping(Log, null);
        _timer?.Change(Timeout.Infinite, 0);

        return _task;
    }

    public void InvokeOnCompleted(Action<ICanStopEarly, bool> listener) {
        _invokeOnCompleted.Add(listener);
    }

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                _timer?.Dispose();
            }

            _timer = null;
            disposedValue = true;
        }
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
