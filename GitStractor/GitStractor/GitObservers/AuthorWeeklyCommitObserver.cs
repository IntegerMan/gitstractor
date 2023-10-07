using GitStractor.Model;
using System.Globalization;

namespace GitStractor.GitObservers;

public class AuthorWeeklyCommitObserver : FileWriterObserverBase {
    public override string Filename => "AuthorWeeklyCommits.csv";
    private readonly Dictionary<int, Dictionary<string, List<CommitInfo>>> _authorCommits = new();

    public override void OnBeginningIteration(int totalCommits, string outputPath, bool includeBranchDetails) {
        base.OnBeginningIteration(totalCommits, outputPath, includeBranchDetails);

        WriteField("Author");
        WriteField("Year");
        WriteField("Week");
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
        int week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(commit.AuthorDateUtc, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        string year_week = year + "_" + week;

        if (!_authorCommits.TryGetValue(commit.Author.Id, out Dictionary<string, List<CommitInfo>>? aggregatedCommits)) {
            aggregatedCommits = new Dictionary<string, List<CommitInfo>>();
            _authorCommits[commit.Author.Id] = aggregatedCommits;
        }

        if (!aggregatedCommits.TryGetValue(year_week, out List<CommitInfo>? value)) {
            value = new List<CommitInfo>();
            aggregatedCommits[year_week] = value;
        }

        value.Add(commit);
    }

    public override void OnCompletedIteration(string outputPath) {
        foreach (KeyValuePair<int, Dictionary<string, List<CommitInfo>>> authorAgg in _authorCommits) {
            foreach (KeyValuePair<string, List<CommitInfo>> kvp in authorAgg.Value) {
                string key = kvp.Key;
                int year = int.Parse(key.AsSpan(0, 4), CultureInfo.InvariantCulture);
                int week = int.Parse(key.AsSpan(5), CultureInfo.InvariantCulture);

                WriteField(authorAgg.Key);

                WriteField(year);
                WriteField(week);
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
