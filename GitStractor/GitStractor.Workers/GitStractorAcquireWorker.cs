using GitStractor.Cloning;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GitStractor.Workers; 

public class GitStractorAcquireWorker : GitStractorWorkerBase {
    private readonly GitStractorAcquireOptions options;
    private readonly RepositoryCloner cloner;

    private static readonly Action<ILogger, Exception?> logExecuting = LoggerMessage.Define(LogLevel.Information, new EventId(1, "RunningExtract"), "Running Extract Task");
    private static readonly Action<ILogger, string, Exception?> logWorkError = LoggerMessage.Define<string>(LogLevel.Error, new EventId(1, "ExtractError"), "Error running extract task: {ErrorMessage}");
    private static readonly Action<ILogger, string, Exception?> logCompleted = LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, "Cloned"), "Repository cloned to {Path}");

    private Timer? _timer;

    public override string Name => "GitStractor-Acquire";

    public const int InitialDelayInSeconds = 2;

    public GitStractorAcquireWorker(ILogger<GitStractorAcquireWorker> log, IOptions<GitStractorAcquireOptions> options, RepositoryCloner cloner) : base(log) {
        this.options = options.Value;
        this.cloner = cloner;
    }

    protected override async Task OnStartAsync() {
        _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(InitialDelayInSeconds), TimeSpan.FromMilliseconds(-1));

        await base.OnStartAsync();
    }

    private void DoWork(object? state) {
        bool succeeded = true;

        try {
            logExecuting(Log, null);

            string finalPath = cloner.Clone(options.Repository, options.ExtractPath, options.OverwriteIfExists);

            logCompleted(Log, finalPath, null);
        }
        catch (CloneException ex) {
            logWorkError(Log, ex.Message, ex);
            succeeded = false;
        }

        NotifyWorkCompleted(succeeded);
    }

    protected override async Task OnStopAsync() {
        _timer?.Change(Timeout.Infinite, 0);

        await base.OnStopAsync();
    }

    protected override void Dispose(bool disposing) {
        if (!HasDisposed) {
            if (disposing) {
                _timer?.Dispose();
            }

            _timer = null;
        }
   }
}
