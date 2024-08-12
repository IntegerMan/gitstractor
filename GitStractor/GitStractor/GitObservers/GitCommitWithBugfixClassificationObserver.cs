using GitStractor.Model;
using Microsoft.Extensions.ML;

namespace GitStractor.GitObservers;

public class GitCommitWithBugfixClassificationObserver : FileWriterObserverBase
{
    private readonly PredictionEnginePool<CommitClassifierInput, CommitClassification> _pool;

    public GitCommitWithBugfixClassificationObserver(PredictionEnginePool<CommitClassifierInput, CommitClassification> pool)
    {
        _pool = pool;
    }
    
    public override string Filename => "Commits.csv";

    public override void OnBeginningIteration(int totalCommits, string outputPath, bool includeBranchDetails)
    {
        base.OnBeginningIteration(totalCommits, outputPath, includeBranchDetails);

        WriteField("Sha");
        WriteField("ParentSha");
        WriteField("Parent2Sha");
        WriteField("IsMerge");
        WriteField("IsBugfix");
        WriteField("BugfixProbability");
        WriteField("AuthorId");
        WriteField("AuthorDateUtc");
        WriteField("CommitterId");
        WriteField("CommitterDateUtc");
        WriteField("Message");
        WriteField("Work Items");
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
        
        CommitClassifierInput input = new()
        {
            Message = commit.Message,
            TotalFiles = commit.TotalFiles,
            ModifiedFiles = commit.FilesModified,
            AddedFiles = commit.FilesAdded,
            DeletedFiles = commit.FilesDeleted,
            IsMerge = commit.IsMerge,
            AddedLines = commit.LinesAdded,
            DeletedLines = commit.LinesDeleted,
            TotalLines = commit.TotalLines,
            // These are things the pipeline could potentially calculate in the future
            HasAddedFiles = commit.FilesAdded > 0,
            HasDeletedFiles = commit.FilesDeleted > 0,
            NetLines = commit.LinesAdded - commit.LinesDeleted,
            WorkItems = commit.WorkItemIdentifiers.Count(),
            MessageLength = commit.Message.Length,
            WordCount = commit.Message.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
        };
        CommitClassification prediction = _pool.Predict(input);
        WriteField(prediction.PredictedLabel);
        WriteField(prediction.Probability);
        
        WriteField(commit.Author.Id);
        WriteField(commit.AuthorDateUtc);
        WriteField(commit.Committer.Id);
        WriteField(commit.CommitterDateUtc);
        WriteField(commit.Message);
        WriteField(commit.WorkItemIdentifiers.Count());
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
