using GitStractor.Model;

namespace GitStractor.Writers;

public abstract class AuthorDataWriter : IDisposable
{
    protected abstract void Dispose(bool disposing);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public abstract void BeginWriting();
    public abstract void WriteAuthors(IEnumerable<AuthorInfo> authors);
}