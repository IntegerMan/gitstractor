using GitStractor.Model;

namespace GitStractor.GitObservers;

public class FileCommitObserver : FileWriterObserverBase
{
    public override string Filename => "FileCommits.csv";

    public override void OnBeginningIteration(int totalCommits, string outputPath)
    {
        base.OnBeginningIteration(totalCommits, outputPath);

        WriteField("Commit");
        WriteField("File Sha");
        WriteField("Type");
        WriteField("Path");
        WriteField("Lines Added");
        WriteField("Lines Deleted");
        WriteField("Current Lines");
        NextRecord();
    }

    public override void OnProcessingFile(RepositoryFileInfo fileInfo, CommitInfo commit)
    {
        base.OnProcessingFile(fileInfo, commit);

        if (commit.Sha != fileInfo.Commit ||
            fileInfo.State == FileState.Final ||
            fileInfo.State == FileState.Ignored ||
            fileInfo.State == FileState.Untracked)
            return;

        WriteField(fileInfo.Commit);
        WriteField(fileInfo.Sha);
        WriteField(fileInfo.State.ToString());
        WriteField(fileInfo.Path);
        WriteField(fileInfo.LinesAdded);
        WriteField(fileInfo.LinesDeleted);
        WriteField(fileInfo.Lines);
        NextRecord();
    }
}
