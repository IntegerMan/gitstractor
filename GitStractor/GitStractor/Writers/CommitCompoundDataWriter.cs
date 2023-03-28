using GitStractor.Model;

namespace GitStractor.Writers;

public class CommitCompoundDataWriter : CommitDataWriter
{
    private readonly CommitDataWriter[] _childWriters;

    public CommitCompoundDataWriter(IEnumerable<CommitDataWriter> childWriters)
    {
        _childWriters = childWriters.ToArray();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (CommitDataWriter writer in _childWriters)
            {
                writer.Dispose();
            }
        }
    }

    public override void BeginWriting()
    {
        foreach (CommitDataWriter writer in _childWriters)
        {
            writer.BeginWriting();
        }
    }

    public override void Write(CommitInfo commit)
    {
        foreach (CommitDataWriter writer in _childWriters)
        {
            writer.Write(commit);
        }
    }
}