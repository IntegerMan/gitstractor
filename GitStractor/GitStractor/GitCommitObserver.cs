using CsvHelper;
using GitStractor.Model;
using System.Globalization;

namespace GitStractor;

public class GitCommitObserver : IGitObserver, IDisposable {
    private CsvWriter? _writer;

    public void OnBeginningIteration(int totalCommits, string outputPath) {
        _writer = new CsvWriter(new StreamWriter(Path.Combine(outputPath, "Commits.csv"), append: false), CultureInfo.InvariantCulture);
        _writer.WriteField("Sha");
        _writer.WriteField("AuthorId");
        _writer.WriteField("AuthorDateUtc");
        _writer.WriteField("CommitterId");
        _writer.WriteField("CommitterDateUtc");
        _writer.WriteField("Message");
        _writer.WriteField("ParentSha");
        _writer.WriteField("Total Files");
        _writer.WriteField("Added Files");
        _writer.WriteField("Deleted Files");
        _writer.NextRecord();
    }

    public void OnNewAuthor(AuthorInfo author) {
    }

    public void OnCompletedIteration(string outputPath) {
        _writer!.Flush();
        _writer.Dispose();
        _writer = null;
    }

    public void OnProcessingCommit(CommitInfo commit, bool isLast) {
        //_writer!.WriteRecord(commit);
        _writer!.WriteField(commit.Sha);
        _writer.WriteField(commit.Author.Id);
        _writer.WriteField(commit.AuthorDateUtc);
        _writer.WriteField(commit.Committer.Id);
        _writer.WriteField(commit.CommitterDateUtc);
        _writer.WriteField(commit.Message);
        _writer.WriteField(commit.ParentSha);
        _writer.WriteField(commit.TotalFiles);
        _writer.WriteField(commit.AddedFiles);
        _writer.WriteField(commit.DeletedFiles);
        _writer.NextRecord();
    }

    public void UpdateProgress(double percent, int commitNum, double totalCommits) {
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
        _writer?.Dispose();
    }
}
