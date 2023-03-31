using GitStractor.Model;

namespace GitStractor.Writers;

public abstract class AuthorDataWriter : DataWriterBase
{
    public abstract void WriteAuthors(IEnumerable<AuthorInfo> authors);
}