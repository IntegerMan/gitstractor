using GitStractor.Model;

namespace GitStractor.GitObservers;

public class AuthorYearlyCommitObserver : FileWriterObserverBase {
    public override string Filename => "AuthorYearlyCommits.csv";
    private readonly Dictionary<int, Dictionary<int, List<CommitInfo>>> _authorCommits = new();

    public override void OnBeginningIteration(int totalCommits, string outputPath, bool includeBranchDetails) {
        base.OnBeginningIteration(totalCommits, outputPath, includeBranchDetails);

        WriteField("Author");
        WriteField("Year");
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

        if (commit.IsMerge && IncludeBranchDetails) return;
        
        int year = commit.AuthorDateUtc.Year;

        if (!_authorCommits.TryGetValue(commit.Author.Id, out Dictionary<int, List<CommitInfo>>? aggregatedCommits)) {
            aggregatedCommits = new Dictionary<int, List<CommitInfo>>();
            _authorCommits[commit.Author.Id] = aggregatedCommits;
        }

        if (!aggregatedCommits.TryGetValue(year, out List<CommitInfo>? value)) {
            value = new List<CommitInfo>();
            aggregatedCommits[year] = value;
        }

        value.Add(commit);
    }

    public override void OnCompletedIteration(string outputPath) {
        foreach (KeyValuePair<int, Dictionary<int, List<CommitInfo>>> authorAgg in _authorCommits) {
            foreach (KeyValuePair<int, List<CommitInfo>> kvp in authorAgg.Value) {
                WriteField(authorAgg.Key);

                WriteField(kvp.Key);
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
