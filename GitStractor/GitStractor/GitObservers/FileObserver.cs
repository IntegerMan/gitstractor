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
        WriteField("Bytes");
        WriteField("Path");
        NextRecord();
    }

    public override void OnProcessingFile(RepositoryFileInfo fileInfo, string commitSha)
    {
        base.OnProcessingFile(fileInfo, commitSha);

        if (fileInfo.State != FileState.Final) return;

        WriteField(fileInfo.Commit);
        WriteField(fileInfo.Sha);
        WriteField(fileInfo.Lines);
        WriteField(fileInfo.Bytes);
        WriteField(fileInfo.Path);
        NextRecord();
    }
}
