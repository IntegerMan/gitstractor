using GitStractor.Model;
using Microsoft.Extensions.ML;

namespace GitStractor.GitObservers;

public class DenormalizedFileCommitWithBugFixClassificationObserver : FileWriterObserverBase
{
    
    private readonly PredictionEnginePool<CommitClassifierInput, CommitClassification> _pool;

    public DenormalizedFileCommitWithBugFixClassificationObserver(PredictionEnginePool<CommitClassifierInput, CommitClassification> pool)
    {
        _pool = pool;
    }

    public override string Filename => "FileCommits.csv";

    public override void OnBeginningIteration(int totalCommits, string outputPath, bool includeBranchDetails)
    {
        base.OnBeginningIteration(totalCommits, outputPath, includeBranchDetails);

        WriteField("Commit");
        WriteField("File Sha");
        WriteField("Type");
        WriteField("Path");
        WriteField("Lines Added");
        WriteField("Lines Deleted");
        WriteField("Current Lines");
        WriteField("Commit Author");
        WriteField("Commit Date Utc");
        WriteField("Commit Message");
        WriteField("Work Items");
        WriteField("IsBugfix");
        WriteField("BugfixProbability");
        NextRecord();
    }

    public override void OnProcessingFile(RepositoryFileInfo fileInfo, CommitInfo commitInfo)
    {
        base.OnProcessingFile(fileInfo, commitInfo);

        if (commitInfo.Sha != fileInfo.Commit ||
            fileInfo.State == FileState.Final ||
            fileInfo.State == FileState.Ignored ||
            fileInfo.State == FileState.Untracked ||
            (commitInfo.IsMerge && IncludeBranchDetails))
            return;

        WriteField(fileInfo.Commit);
        WriteField(fileInfo.Sha);
        WriteField(fileInfo.State.ToString());
        WriteField(fileInfo.Path);
        WriteField(fileInfo.LinesAdded);
        WriteField(fileInfo.LinesDeleted);
        WriteField(fileInfo.Lines);
        WriteField(commitInfo.Author.Id);
        WriteField(commitInfo.AuthorDateUtc);
        WriteField(commitInfo.Message);
        WriteField(commitInfo.WorkItemIdentifiers.Count());
        
        CommitClassifierInput input = new()
        {
            Message = commitInfo.Message,
            TotalFiles = commitInfo.TotalFiles,
            ModifiedFiles = commitInfo.FilesModified,
            AddedFiles = commitInfo.FilesAdded,
            DeletedFiles = commitInfo.FilesDeleted,
            IsMerge = commitInfo.IsMerge,
            AddedLines = commitInfo.LinesAdded,
            DeletedLines = commitInfo.LinesDeleted,
            TotalLines = commitInfo.TotalLines,
            // These are things the pipeline could potentially calculate in the future
            HasAddedFiles = commitInfo.FilesAdded > 0,
            HasDeletedFiles = commitInfo.FilesDeleted > 0,
            NetLines = commitInfo.LinesAdded - commitInfo.LinesDeleted,
            WorkItems = commitInfo.WorkItemIdentifiers.Count(),
            MessageLength = commitInfo.Message.Length,
            WordCount = commitInfo.Message.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
        };
        CommitClassification prediction = _pool.Predict(input);
        WriteField(prediction.PredictedLabel);
        WriteField(prediction.Probability);        
        NextRecord();
    }
}
