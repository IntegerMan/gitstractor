using GitStractor.Model;

namespace GitStractor.GitObservers;

public class CommitWorkItemObserver : FileWriterObserverBase
{
    public override string Filename => "CommitWorkItems.csv";

    public override void OnBeginningIteration(int totalCommits, string outputPath, bool includeBranchDetails)
    {
        base.OnBeginningIteration(totalCommits, outputPath, includeBranchDetails);

        WriteField("Commit Sha");
        WriteField("Work Item Id");
        NextRecord();
    }

    public override void OnProcessedCommit(CommitInfo commit)
    {
        base.OnProcessedCommit(commit);

        foreach (string workItemId in commit.WorkItemIdentifiers) {
            WriteField(commit.Sha);
            WriteField(workItemId);
            NextRecord();
        }
    }
}
