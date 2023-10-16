using GitStractor.Model;

namespace GitStractor.GitObservers;

public class GitCommitObserver : FileWriterObserverBase
{
    public override string Filename => "Commits.csv";

    public override void OnBeginningIteration(int totalCommits, string outputPath, bool includeBranchDetails)
    {
        base.OnBeginningIteration(totalCommits, outputPath, includeBranchDetails);

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
        WriteField("Modified Files");
        WriteField("Added Files");
        WriteField("Deleted Files");
        WriteField("Total Lines");
        WriteField("Net Lines");
        WriteField("Added Lines");
        WriteField("Deleted Lines");
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
        WriteField(commit.FilesModified);
        WriteField(commit.FilesAdded);
        WriteField(commit.FilesDeleted);
        WriteField(commit.TotalLines);
        WriteField(commit.Files.Sum(f => f.LinesAdded - f.LinesDeleted));
        WriteField(commit.Files.Sum(f => f.LinesAdded));
        WriteField(commit.Files.Sum(f => f.LinesDeleted));
        NextRecord();
    }
}
