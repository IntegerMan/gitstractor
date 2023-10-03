using GitStractor.Model;
using System.Globalization;

namespace GitStractor.GitObservers;

public class AuthorMonthlyCommitObserver : FileWriterObserverBase {
    public override string Filename => "AuthorMonthlyCommits.csv";
    private readonly Dictionary<int, Dictionary<string, List<CommitInfo>>> _authorCommits = new();

    public override void OnBeginningIteration(int totalCommits, string outputPath) {
        base.OnBeginningIteration(totalCommits, outputPath);

        WriteField("Author");
        WriteField("Year");
        WriteField("Month");
        WriteField("Num Commits");
        WriteField("Lines Added");
        WriteField("Lines Deleted");
        WriteField("Files Added");
        WriteField("Files Deleted");
        WriteField("Files Modified");
        WriteField("First Commit Date UTC");
        WriteField("Last Commit Date UTC");
        NextRecord();
    }

    public override void OnProcessedCommit(CommitInfo commit) {
        base.OnProcessedCommit(commit);

        int year = commit.AuthorDateUtc.Year;
        int month = commit.AuthorDateUtc.Month;
        string year_quarter = year + "_" + month;

        if (!_authorCommits.TryGetValue(commit.Author.Id, out Dictionary<string, List<CommitInfo>>? aggregatedCommits)) {
            aggregatedCommits = new Dictionary<string, List<CommitInfo>>();
            _authorCommits[commit.Author.Id] = aggregatedCommits;
        }

        if (!aggregatedCommits.TryGetValue(year_quarter, out List<CommitInfo>? value)) {
            value = new List<CommitInfo>();
            aggregatedCommits[year_quarter] = value;
        }

        value.Add(commit);
    }

    public override void OnCompletedIteration(string outputPath) {
        foreach (KeyValuePair<int, Dictionary<string, List<CommitInfo>>> authorAgg in _authorCommits) {
            foreach (KeyValuePair<string, List<CommitInfo>> kvp in authorAgg.Value) {
                string key = kvp.Key;
                int year = int.Parse(key.AsSpan(0, 4), CultureInfo.InvariantCulture);
                int month = int.Parse(key.AsSpan(5), CultureInfo.InvariantCulture);

                WriteField(authorAgg.Key);

                WriteField(year);
                WriteField(month);
                WriteField(kvp.Value.Count);
                WriteField(kvp.Value.Sum(c => c.LinesAdded));
                WriteField(kvp.Value.Sum(c => c.LinesDeleted));
                WriteField(kvp.Value.Sum(c => c.FilesAdded));
                WriteField(kvp.Value.Sum(c => c.FilesDeleted));
                WriteField(kvp.Value.Sum(c => c.FilesModified));
                WriteField(kvp.Value.Min(c => c.AuthorDateUtc));
                WriteField(kvp.Value.Max(c => c.AuthorDateUtc));
                NextRecord();
            }
        }

        base.OnCompletedIteration(outputPath);
    }
}
