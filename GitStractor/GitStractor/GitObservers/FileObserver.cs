using CsvHelper;
using GitStractor.Model;
using System.Globalization;

namespace GitStractor.GitObservers;

public class FileObserver : FileWriterObserverBase
{
    public override string Filename => "Files.csv";

    public override void OnProcessingCommit(string sha, bool isLast)
    {
        base.OnProcessingCommit(sha, isLast);

        if (!isLast) return;

        WriteField("Commit Sha");
        WriteField("File Sha");
        WriteField("Lines");
        WriteField("Path");
        WriteField("Path1");
        WriteField("Path2");
        WriteField("Path3");
        WriteField("Extension");
        NextRecord();
    }

    public override void OnProcessingFile(RepositoryFileInfo fileInfo, CommitInfo commit)
    {
        base.OnProcessingFile(fileInfo, commit);

        if (fileInfo.State != FileState.Final) return;

        WriteField(fileInfo.Commit);
        WriteField(fileInfo.Sha);
        WriteField(fileInfo.Lines);
        WriteField(fileInfo.Path);
        WriteField(fileInfo.Path1);
        WriteField(fileInfo.Path2);
        WriteField(fileInfo.Path3);
        WriteField(fileInfo.Extension);
        NextRecord();
    }
}
