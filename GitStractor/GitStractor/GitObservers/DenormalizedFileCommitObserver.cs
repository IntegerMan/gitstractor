using GitStractor.Model;

namespace GitStractor.GitObservers;

public class DenormalizedFileCommitObserver : FileWriterObserverBase
{
    public override string Filename => "FileCommits.csv";

    public override void OnBeginningIteration(int totalCommits, string outputPath)
    {
        base.OnBeginningIteration(totalCommits, outputPath);

        WriteField("Commit");
        WriteField("Type");
        WriteField("Path");
        WriteField("Lines Added");
        WriteField("Lines Deleted");
        WriteField("Current Lines");
        WriteField("Commit Author");
        WriteField("Commit Date UTC");
        WriteField("Commit Message");
        NextRecord();
    }

    public override void OnProcessingFile(RepositoryFileInfo fileInfo, CommitInfo commitInfo)
    {
        base.OnProcessingFile(fileInfo, commitInfo);

        if (commitInfo.Sha != fileInfo.Commit ||
            fileInfo.State == FileState.Final ||
            fileInfo.State == FileState.Ignored ||
            fileInfo.State == FileState.Untracked)
            return;

        WriteField(fileInfo.Commit);
        WriteField(fileInfo.State.ToString());
        WriteField(fileInfo.Path);
        WriteField(fileInfo.LinesAdded);
        WriteField(fileInfo.LinesDeleted);
        WriteField(fileInfo.Lines);
        WriteField(commitInfo.Author.Id);
        WriteField(commitInfo.AuthorDateUtc);
        WriteField(commitInfo.Message);
        NextRecord();
    }
}
