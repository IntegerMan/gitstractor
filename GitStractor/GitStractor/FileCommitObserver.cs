using CsvHelper;
using GitStractor.Model;
using LibGit2Sharp;
using System.Globalization;

namespace GitStractor;

public class FileCommitObserver : IGitObserver, IDisposable {
    private CsvWriter? _writer;
    private string _outputPath = Environment.CurrentDirectory;

    public void OnBeginningIteration(int totalCommits, string outputPath) {
        _outputPath = outputPath;
    }

    public void OnNewAuthor(AuthorInfo author) {
    }

    public void OnCompletedIteration(string outputPath) {
    }

    public void OnProcessingCommit(string sha, bool isLast) {
        _writer = new CsvWriter(new StreamWriter(Path.Combine(_outputPath, $"Files_{sha}.csv"), append: false), CultureInfo.InvariantCulture);
        _writer.WriteField("CommitSha");
        _writer.WriteField("FileSha");
        _writer.WriteField("Path");
        _writer.NextRecord();
    }

    public void OnProcessedCommit(CommitInfo commit) {
        _writer!.Flush();
        _writer.Dispose();
        _writer = null;
    }

    public void OnProcessingFile(RepositoryFileInfo fileInfo) {
        _writer!.WriteField(fileInfo.Commit);
        _writer.WriteField(fileInfo.Sha);
        _writer.WriteField(fileInfo.Path);
        _writer.NextRecord();
    }

    public void UpdateProgress(double percent, int commitNum, double totalCommits) {
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
        _writer?.Dispose();
    }


}
