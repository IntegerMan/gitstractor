using CsvHelper;
using GitStractor.Model;
using System.Globalization;

namespace GitStractor;

public class GitAuthorObserver : IGitObserver, IDisposable {
    private CsvWriter? _writer;

    public void OnBeginningIteration(int totalCommits, string outputPath) {
        _writer = new CsvWriter(new StreamWriter(Path.Combine(outputPath, "Authors.csv"), append: false), CultureInfo.InvariantCulture);

        _writer.WriteField("Id");
        _writer.WriteField("Email");
        _writer.WriteField("Name");
        _writer.WriteField("IsBot");
        _writer.WriteField("FirstCommitDateUTC");
        _writer.NextRecord();
    }

    public void OnNewAuthor(AuthorInfo author) {
        _writer!.WriteField(author.Id);
        _writer.WriteField(author.Email);
        _writer.WriteField(author.Name);
        _writer.WriteField(author.IsBot);
        _writer.WriteField(author.EarliestCommitDateUtc);
        _writer.NextRecord();
    }

    public void OnCompletedIteration(string outputPath) {
        _writer!.Flush();
        _writer.Dispose();
        _writer = null;
    }

    public void OnProcessingCommit(CommitInfo commit, bool isLast) {
    }

    public void UpdateProgress(double percent, int commitNum, double totalCommits) {
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
        _writer?.Dispose();
    }
}