using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitStractor.Acquire; 
public class GitStractorAcquisitionWorker : IHostedService, IAsyncDisposable {
    private readonly GitStractorAcquireOptions options;
    private readonly RepositoryCloner cloner;

    private readonly Action<ILogger, Exception?> logStarted = LoggerMessage.Define(LogLevel.Debug, new EventId(1, "StartingClone"), "Registering Clone Task");
    private readonly Action<ILogger, Exception?> logExecuting = LoggerMessage.Define(LogLevel.Debug, new EventId(1, "RunningClone"), "Running Clone Task");
    private readonly Action<ILogger, Exception?> logStopping = LoggerMessage.Define(LogLevel.Debug, new EventId(1, "StoppingClone"), "Stopping Clone Task");
    private readonly Action<ILogger, string, Exception?> logCloned = LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, "Cloned"), "Repository cloned to {Path}");

    private Timer? _timer;
    private CancellationToken _token;
    private Task _task = Task.CompletedTask;
    public ILogger<GitStractorAcquisitionWorker> Log { get; }

    public const int InitialDelayInSeconds = 2;

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
        try {
            logExecuting(Log, null);

            string finalPath = cloner.Clone(options.Repository, options.ExtractPath, options.OverwriteIfExists);

            logCloned(Log, finalPath, null);
        }
        catch (CloneException ex) {
            Log.LogError(ex.Message, ex);
        }

        StopAsync(_token);
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        logStopping(Log, null);
        _timer?.Change(Timeout.Infinite, 0);

        return _task;
    }

    public async ValueTask DisposeAsync() {
        if (_timer is IAsyncDisposable timer) {
            await timer.DisposeAsync();
        }

        _timer = null;
    }
}
