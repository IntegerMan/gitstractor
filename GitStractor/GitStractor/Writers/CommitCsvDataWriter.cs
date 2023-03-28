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
        _writer.WriteLine("CommitHash,AuthorEmail,AuthorDateUTC,CommitterEmail,CommitterDate,Message,NumFiles,TotalBytes,FileNames");
    }

    public override void Write(CommitInfo commit)
    {
        if (_writer == null) throw new InvalidOperationException("The writer is not currently open");

        _writer.Write($"{commit.Sha},");
        _writer.Write($"{commit.Author.Email.ToCsvSafeString()},{commit.AuthorDateUtc},");
        _writer.Write($"{commit.Committer.Email.ToCsvSafeString()},{commit.CommitterDateUtc},");
        _writer.Write($"{commit.Message.ToCsvSafeString()},");
        _writer.WriteLine($"{commit.NumFiles},{commit.SizeInBytes},{commit.FileNames.ToCsvSafeString()}");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _writer?.Dispose();
        }
    }
}