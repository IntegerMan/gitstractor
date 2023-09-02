using CsvHelper;
using GitStractor.Model;
using System.Globalization;

namespace GitStractor.GitObservers;

public class FileCommitObserver : IGitObserver, IDisposable
{
    private CsvWriter? _writer;

    public void OnBeginningIteration(int totalCommits, string outputPath)
    {
        _writer = new CsvWriter(new StreamWriter(Path.Combine(outputPath, "FileCommits.csv"), append: false), CultureInfo.InvariantCulture);
        _writer.WriteField("Commit");
        _writer.WriteField("Type");
        _writer.WriteField("Path");
        _writer.WriteField("Lines Added");
        _writer.WriteField("Lines Deleted");
        _writer.NextRecord();
    }

    public void OnNewAuthor(AuthorInfo author)
    {
    }

    public void OnCompletedIteration(string outputPath)
    {
    }

    public void OnProcessingCommit(string sha, bool isLast)
    {
    }

    public void OnProcessedCommit(CommitInfo commit)
    {
    }

    public void OnProcessingFile(RepositoryFileInfo fileInfo, string commitSha)
    {
        if (_writer == null ||
            commitSha != fileInfo.Commit ||
            fileInfo.State == FileState.Final ||
            fileInfo.State == FileState.Ignored ||
            fileInfo.State == FileState.Untracked)
            return;

        _writer.WriteField(fileInfo.Commit);
        _writer.WriteField(fileInfo.State.ToString());
        _writer.WriteField(fileInfo.Path);
        _writer.WriteField(fileInfo.LinesAdded);
        _writer.WriteField(fileInfo.LinesDeleted);
        _writer.NextRecord();
    }

    public void UpdateProgress(double percent, int commitNum, double totalCommits)
    {
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _writer?.Dispose();
        _writer = null;
    }
}
