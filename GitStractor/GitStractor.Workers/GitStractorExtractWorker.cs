using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GitStractor.Workers;

public class GitStractorExtractWorker : GitStractorWorkerBase {
    private readonly GitStractorExtractOptions _options;
    private readonly GitDataExtractor _extractor;

    private static readonly Action<ILogger, Exception?> logExecuting = LoggerMessage.Define(LogLevel.Information, new EventId(1, "RunningExtract"), "Running Extract Task");
    private static readonly Action<ILogger, string, Exception?> logWorkError = LoggerMessage.Define<string>(LogLevel.Error, new EventId(1, "ExtractError"), "Error running extract task: {ErrorMessage}");
    private static readonly Action<ILogger, string, Exception?> logExtracted = LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, "Extracted"), "Repository information extracted from {Path}");
    private Timer? _timer;

    public override string Name => "GitStractor-Extract";

    public const int InitialDelayInMilliseconds = 200;

    public GitStractorExtractWorker(ILogger<GitStractorExtractWorker> log, IOptions<GitStractorExtractOptions> options, GitDataExtractor extractor) : base(log) {
        _options = options.Value;
        _extractor = extractor;
    }

    protected override async Task OnStartAsync() {
        // TODO: This belongs in a new base class
        _timer = new Timer(DoWork, null, TimeSpan.FromMilliseconds(InitialDelayInMilliseconds), TimeSpan.FromMilliseconds(-1));

        await base.OnStartAsync();
    }

    protected override Task OnStopAsync() {
        _timer?.Change(Timeout.Infinite, 0);

        return base.OnStopAsync();
    }

    private void DoWork(object? state) {
        bool succeeded = true;

        try {
            logExecuting(Log, null);

            // Create output directory
            // TODO: only do this if doesn't exist
            Directory.CreateDirectory(_options.OutputPath);

            // TODO: fail if any file exists and we're not set to overwrite

            // Extract
            /*
            GitExtractionOptions extractOptions = new() {
                RepositoryPath = _options.SourcePath,
                AuthorWriter = new AuthorCsvDataWriter(Path.Combine(_options.OutputPath, "Authors.csv")),
                CommitWriter = new CommitCsvDataWriter(Path.Combine(_options.OutputPath, "Commits.csv")),
                FileWriter = new FileCsvDataWriter(Path.Combine(_options.OutputPath, "Files.csv")),
            };
            */
            _extractor.ExtractInformation(_options.SourcePath, _options.OutputPath);

            logExtracted(Log, _options.OutputPath, null);

            // TODO: pass this off to something else, potentially, for analysis

        }
        catch (InvalidOperationException ex) { // TODO: Revisit
            logWorkError(Log, ex.Message, ex);
            succeeded = false;
        }

        NotifyWorkCompleted(succeeded);
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
