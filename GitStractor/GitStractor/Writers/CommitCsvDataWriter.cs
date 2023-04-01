using GitStractor.Model;
using GitStractor.Utilities;

namespace GitStractor.Writers;

public class CommitCsvDataWriter : CommitDataWriter
{
    private readonly string _path;
    private StreamWriter? _writer;

    public CommitCsvDataWriter(string path)
    {
        _path = path ?? throw new ArgumentNullException(nameof(path));
    }

    public override void BeginWriting()
    {
        if (_writer != null) throw new InvalidOperationException("The writer is already open");

        // Using not needed here since we're IDisposable
        _writer = new StreamWriter(_path, append: false);

        // Write the header row
        WriteHeaderRow(_writer);
    }

    protected virtual void WriteHeaderRow(StreamWriter writer)
    {
        writer.WriteLine("CommitHash,AuthorEmail,AuthorDateUTC,CommitterEmail,CommitterDate,Message,NumFiles,AddedFiles,DeletedFiles,TotalFiles,TotalBytes,FileNames");
    }

    public sealed override void Write(CommitInfo commit)
    {
        if (_writer == null) throw new InvalidOperationException("The writer is not currently open");

        WriteRow(commit, _writer);
    }

    protected virtual void WriteRow(CommitInfo commit, TextWriter writer)
    {
        writer.Write($"{commit.Sha},");
        writer.Write($"{commit.Author.Email.ToCsvSafeString()},{commit.AuthorDateUtc},");
        writer.Write($"{commit.Committer.Email.ToCsvSafeString()},{commit.CommitterDateUtc},");
        writer.Write($"{commit.Message.ToCsvSafeString()},");
        writer.WriteLine(
            $"{commit.NumFiles},{commit.AddedFiles},{commit.DeletedFiles},{commit.TotalFiles},{commit.SizeInBytes},{commit.FileNames.ToCsvSafeString()}");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _writer?.Dispose();
        }
    }
}