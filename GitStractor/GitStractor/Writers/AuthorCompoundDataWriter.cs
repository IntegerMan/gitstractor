using GitStractor.Model;

namespace GitStractor.Writers;

public class AuthorCompoundDataWriter : AuthorDataWriter
{
    private readonly AuthorDataWriter[] _childWriters;

    public AuthorCompoundDataWriter(IEnumerable<AuthorDataWriter> childWriters)
    {
        _childWriters = childWriters.ToArray();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (AuthorDataWriter writer in _childWriters)
            {
                writer.Dispose();
            }
        }
    }

    public override void BeginWriting()
    {
        foreach (AuthorDataWriter writer in _childWriters)
        {
            writer.BeginWriting();
        }
    }

    public override void WriteAuthors(IEnumerable<AuthorInfo> authors)
    {
        foreach (AuthorDataWriter writer in _childWriters)
        {
            writer.WriteAuthors(authors);
        }
    }
}