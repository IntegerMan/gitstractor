using GitStractor.Model;
using GitStractor.Utilities;

namespace GitStractor.Writers;

public class FileCsvDataWriter : FileDataWriter
{
    private readonly string _path;
    private StreamWriter? _writer;

    public FileCsvDataWriter(string path)
    {
        _path = path ?? throw new ArgumentNullException(nameof(path));
    }

    public override void BeginWriting()
    {
        if (_writer != null) throw new InvalidOperationException("The writer is already open");

        // Using not needed here since we're IDisposable
        _writer = new StreamWriter(_path, append: false);

        // Write the header row
        _writer.WriteLine("CommitHash,AuthorEmail,AuthorDateUTC,CommitterEmail,CommitterDate,Message,NumFiles,AddedFiles,DeletedFiles,TotalFiles,TotalBytes,FileNames");
    }

    public override void WriteFile(RepositoryFileInfo fileInfo)
    {
        if (_writer == null) throw new InvalidOperationException("The writer is not currently open");

        // TODO: Write it to disk!
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _writer?.Dispose();
        }
    }
}