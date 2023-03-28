namespace GitStractor.Writers;

public abstract class DataWriterBase : IDisposable
{
    protected abstract void Dispose(bool disposing);

    public virtual void BeginWriting()
    {
        // Do nothing by default, but child classes can customize this
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }    
}