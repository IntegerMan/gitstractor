using GitStractor.Model;

namespace GitStractor.Writers;

public abstract class FileDataWriter : DataWriterBase
{
    public abstract void WriteFile(RepositoryFileInfo file);
}