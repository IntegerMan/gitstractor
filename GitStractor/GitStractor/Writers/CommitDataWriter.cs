using GitStractor.Model;

namespace GitStractor.Writers;

public abstract class CommitDataWriter : DataWriterBase
{
    public abstract void Write(CommitInfo commitInfo);
}