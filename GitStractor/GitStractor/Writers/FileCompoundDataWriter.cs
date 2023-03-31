using GitStractor.Model;

namespace GitStractor.Writers;

public class FileCompoundDataWriter : FileDataWriter
{
    private readonly FileDataWriter[] _childWriters;

    public FileCompoundDataWriter(IEnumerable<FileDataWriter> childWriters)
    {
        _childWriters = childWriters.ToArray();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (FileDataWriter writer in _childWriters)
            {
                writer.Dispose();
            }
        }
    }

    public override void BeginWriting()
    {
        foreach (FileDataWriter writer in _childWriters)
        {
            writer.BeginWriting();
        }
    }

    public override void WriteFile(RepositoryFileInfo fileInfo)
    {
        foreach (FileDataWriter writer in _childWriters)
        {
            writer.WriteFile(fileInfo);
        }
    }
}