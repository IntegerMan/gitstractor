using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GitStractor.Workers;

public abstract class GitStractorWorkerBase : IHostedService, ICanStopEarly, IDisposable {

    private static readonly Action<ILogger, string, Exception?> logStarted = LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, "StartingWorker"), "Registering {Worker} Task");
    private static readonly Action<ILogger, string, Exception?> logStopping = LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, "StoppingWorker"), "Stopping {Worker} Task");

    public ILogger<GitStractorWorkerBase> Log { get; private set; }


    public GitStractorWorkerBase(ILogger<GitStractorWorkerBase> log) {
        Log = log;
    }

    public abstract string Name { get; }
    private readonly List<Action<ICanStopEarly, bool>> _invokeOnCompleted = new();

    public async Task StartAsync(CancellationToken cancellationToken) {
        logStarted(Log, Name, null);

        await OnStartAsync();
    }

    protected virtual async Task OnStartAsync() { 
        await Task.CompletedTask; 
    }

    public async Task StopAsync(CancellationToken cancellationToken) {
        logStopping(Log, Name, null);

        await OnStopAsync();
    }


    protected virtual async Task OnStopAsync() { 
        await Task.CompletedTask; 
    }


    public void InvokeOnCompleted(Action<ICanStopEarly, bool> listener) {
        _invokeOnCompleted.Add(listener);
    }

    protected void NotifyWorkCompleted(bool succeeded) {
        foreach (var listener in _invokeOnCompleted) {
            listener.Invoke(this, succeeded);
        }
    }


    public bool HasDisposed { get; private set; }

    protected abstract void Dispose(bool disposing);

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        HasDisposed = true;
        GC.SuppressFinalize(this);
    }
}
