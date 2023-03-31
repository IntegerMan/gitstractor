using GitStractor.Model;
using GitStractor.Utilities;

namespace GitStractor.Writers;

public class FileCsvDataWriter : FileDataWriter
{
    private readonly string _path;
    private readonly FileState _statesToWrite;
    private StreamWriter? _writer;

    public FileCsvDataWriter(string path, FileState statesToWrite = FileState.Added | FileState.Deleted | FileState.Modified)
    {
        _path = path ?? throw new ArgumentNullException(nameof(path));
        _statesToWrite = statesToWrite;
    }

    public override void BeginWriting()
    {
        if (_writer != null) throw new InvalidOperationException("The writer is already open");

        // Using not needed here since we're IDisposable
        _writer = new StreamWriter(_path, append: false);

        // Write the header row
        _writer.WriteLine("CommitHash,FileHash,Filename,Extension,FilePath,State,Bytes,CreatedDateUTC");
    }

    public override void WriteFile(RepositoryFileInfo fileInfo)
    {
        if (_writer == null) throw new InvalidOperationException("The writer is not currently open");

        // Only write the file if it's in the states we're interested in
        if (!_statesToWrite.HasFlag(fileInfo.State)) return;
        
        _writer.Write($"{fileInfo.Commit},{fileInfo.Sha},{fileInfo.Name.ToCsvSafeString()},{fileInfo.Extension},");
        _writer.WriteLine($"{fileInfo.Path.ToCsvSafeString()},{fileInfo.State},{fileInfo.Bytes},{fileInfo.CreatedDateUtc}");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _writer?.Dispose();
        }
    }
}