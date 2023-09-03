using CsvHelper;
using GitStractor.Model;
using System.Globalization;

namespace GitStractor.GitObservers;

public abstract class FileWriterObserverBase : IGitObserver, IDisposable {
    private CsvWriter? _writer;

    public void Dispose() {
        GC.SuppressFinalize(this);
        _writer?.Dispose();
        _writer = null;
    }

    public abstract string Filename { get; }

    public virtual void OnBeginningIteration(int totalCommits, string outputPath) {
        _writer = new CsvWriter(new StreamWriter(Path.Combine(outputPath, Filename), append: false), CultureInfo.InvariantCulture);
    }

    public virtual void OnCompletedIteration(string outputPath) {
        if (_writer != null) {
            _writer.Dispose();
            _writer = null;
        }
    }

    public virtual void OnNewAuthor(AuthorInfo author) {
    }

    public virtual void OnProcessedCommit(CommitInfo commit) {
    }

    public virtual void OnProcessingCommit(string sha, bool isLast) {
    }

    public virtual void OnProcessingFile(RepositoryFileInfo fileInfo, CommitInfo commitInfo) {
    }

    public virtual void UpdateProgress(double percent, int commitNum, double totalCommits) {
    }

    protected void NextRecord() {
        if (_writer == null) {
            throw new InvalidOperationException("Cannot write to a file before the writer is open");
        }
        _writer.NextRecord();
    }

    protected void WriteField(object? value) {
        if (_writer == null) {
            throw new InvalidOperationException("Cannot write to a file before the writer is open");
        }
        _writer.WriteField(value);
    }
}
