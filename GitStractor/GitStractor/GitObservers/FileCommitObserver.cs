using GitStractor.Model;

namespace GitStractor.GitObservers;

public class FileCommitObserver : FileWriterObserverBase
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
        NextRecord();
    }

    public override void OnProcessingFile(RepositoryFileInfo fileInfo, string commitSha)
    {
        base.OnProcessingFile(fileInfo, commitSha);

        if (commitSha != fileInfo.Commit ||
            fileInfo.State == FileState.Final ||
            fileInfo.State == FileState.Ignored ||
            fileInfo.State == FileState.Untracked)
            return;

        WriteField(fileInfo.Commit);
        WriteField(fileInfo.State.ToString());
        WriteField(fileInfo.Path);
        WriteField(fileInfo.LinesAdded);
        WriteField(fileInfo.LinesDeleted);
        NextRecord();
    }
}
