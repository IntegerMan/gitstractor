using GitStractor.Model;

namespace GitStractor.GitObservers;

public class GitCommitObserver : FileWriterObserverBase
{
    public override string Filename => "Commits.csv";

    public override void OnBeginningIteration(int totalCommits, string outputPath)
    {
        base.OnBeginningIteration(totalCommits, outputPath);

        WriteField("Sha");
        WriteField("ParentSha");
        WriteField("Parent2Sha");
        WriteField("IsMerge");
        WriteField("AuthorId");
        WriteField("AuthorDateUtc");
        WriteField("CommitterId");
        WriteField("CommitterDateUtc");
        WriteField("Message");
        WriteField("Total Files");
        WriteField("Added Files");
        WriteField("Deleted Files");
        NextRecord();
    }

    public override void OnProcessedCommit(CommitInfo commit)
    {
        base.OnProcessedCommit(commit);

        WriteField(commit.Sha);
        WriteField(commit.ParentSha);
        WriteField(commit.Parent2Sha);
        WriteField(commit.IsMerge);
        WriteField(commit.Author.Id);
        WriteField(commit.AuthorDateUtc);
        WriteField(commit.Committer.Id);
        WriteField(commit.CommitterDateUtc);
        WriteField(commit.Message);
        WriteField(commit.TotalFiles);
        WriteField(commit.FilesAdded);
        WriteField(commit.FilesDeleted);
        NextRecord();
    }
}
