using GitStractor.Model;

namespace GitStractor.GitObservers;

public class AuthorDailyCommitObserver : FileWriterObserverBase {
    public override string Filename => "AuthorDailyCommits.csv";
    private readonly Dictionary<int, Dictionary<DateTime, List<CommitInfo>>> _authorCommits = new();

    public override void OnBeginningIteration(int totalCommits, string outputPath) {
        base.OnBeginningIteration(totalCommits, outputPath);

        WriteField("Author");
        WriteField("Date");
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

        DateTime dt = commit.AuthorDateUtc.Date;

        if (!_authorCommits.TryGetValue(commit.Author.Id, out Dictionary<DateTime, List<CommitInfo>>? aggregatedCommits)) {
            aggregatedCommits = new Dictionary<DateTime, List<CommitInfo>>();
            _authorCommits[commit.Author.Id] = aggregatedCommits;
        }

        if (!aggregatedCommits.TryGetValue(dt, out List<CommitInfo>? value)) {
            value = new List<CommitInfo>();
            aggregatedCommits[dt] = value;
        }

        value.Add(commit);
    }

    public override void OnCompletedIteration(string outputPath) {
        foreach (KeyValuePair<int, Dictionary<DateTime, List<CommitInfo>>> authorAgg in _authorCommits) {
            foreach (KeyValuePair<DateTime, List<CommitInfo>> kvp in authorAgg.Value) {
                WriteField(authorAgg.Key);

                WriteField(kvp.Key.ToShortDateString());
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
