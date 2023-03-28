using GitStractor.Model;

namespace GitStractor.Writers;

public class CommitConsoleDataWriter : CommitDataWriter
{
    protected override void Dispose(bool disposing)
    {
        // Nothing to dispose
    }

    public override void Write(CommitInfo commit) => Console.WriteLine(commit);
}