using CsvHelper;
using GitStractor.Model;
using System.Globalization;

namespace GitStractor;

public class FileObserver : IGitObserver, IDisposable {
    private string _outputPath = Environment.CurrentDirectory;
    private CsvWriter? _writer;

    public void OnBeginningIteration(int totalCommits, string outputPath) {
        _outputPath = outputPath;
    }

    public void OnNewAuthor(AuthorInfo author) {
    }

    public void OnCompletedIteration(string outputPath) {
    }

    public void OnProcessingCommit(string sha, bool isLast) {
        if (!isLast) return;

        _writer = new CsvWriter(new StreamWriter(Path.Combine(_outputPath, "Files.csv"), append: false), CultureInfo.InvariantCulture);
        _writer.WriteField("CommitSha");
        _writer.WriteField("FileSha");
        _writer.WriteField("Path");
        _writer.NextRecord();
    }

    public void OnProcessedCommit(CommitInfo commit) {
    }

    public void OnProcessingFile(RepositoryFileInfo fileInfo) {
        if (_writer == null) return;

        _writer.WriteField(fileInfo.Commit);
        _writer.WriteField(fileInfo.Sha);
        _writer.WriteField(fileInfo.Path);
        _writer.NextRecord();
    }

    public void UpdateProgress(double percent, int commitNum, double totalCommits) {
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
        _writer?.Dispose();
        _writer = null;
    }
}
