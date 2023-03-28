using GitStractor.Model;

namespace GitStractor.Writers;

public class AuthorCsvDataWriter : AuthorDataWriter
{
    private readonly string _path;
    private StreamWriter? _writer;

    public AuthorCsvDataWriter(string path)
    {

        _path = path ?? throw new ArgumentNullException(nameof(path));
    }

    public override void BeginWriting()
    {
        if (_writer != null) throw new InvalidOperationException("The writer is already open");

        // Using not needed here since we're IDisposable
        _writer = new StreamWriter(_path, append: false);

        // Write the header row
        _writer.WriteLine("Name,Email,NumCommits,TotalBytes");
    }

    public override void WriteAuthors(IEnumerable<AuthorInfo> authors)
    {
        if (_writer == null) throw new InvalidOperationException("The writer is not currently open");

        foreach (AuthorInfo author in authors)
        {
            _writer!.WriteLine($"{author.Name},{author.Email},{author.NumCommits},{author.TotalSizeInBytes}");
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _writer?.Dispose();
        }
    }
}