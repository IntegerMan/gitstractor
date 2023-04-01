using GitStractor.Model;
using GitStractor.Utilities;

namespace GitStractor.Writers;

public class FileCommitCsvDataWriter : CommitCsvDataWriter
{
    public FileCommitCsvDataWriter(string path) : base(path)
    {
    }

    protected override void WriteHeaderRow(StreamWriter writer)
    {
        writer!.WriteLine("FilePath,FileHash,CommitHash,AuthorEmail,AuthorDateUTC,CommitterEmail,CommitterDateUTC,Message,Bytes,Lines,NetLines");
    }

    protected override void WriteRow(CommitInfo commit, TextWriter writer)
    {
        foreach (var file in commit.Files)
        {
            writer.Write($"{file.Path.ToCsvSafeString()},{file.Sha},{commit.Sha},");
            writer.Write($"{commit.Author.Email.ToCsvSafeString()},{commit.AuthorDateUtc},");
            writer.Write($"{commit.Committer.Email.ToCsvSafeString()},{commit.CommitterDateUtc},");
            writer.Write($"{commit.Message.ToCsvSafeString()},");
            writer.WriteLine($"{file.Bytes},{file.Lines},{file.LinesDelta}");
        }
    }
}