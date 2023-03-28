using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitStractor.Model;

namespace GitStractor.Writers;

public class CommitInMemoryDataWriter : CommitDataWriter, IEnumerable<CommitInfo>
{
    private readonly List<CommitInfo> _commits = new();

    protected override void Dispose(bool disposing)
    {
        // Nothing to dispose
    }

    public override void BeginWriting() => _commits.Clear();

    public override void Write(CommitInfo commit) => _commits.Add(commit);

    public IEnumerable<CommitInfo> Commits => _commits.AsReadOnly();
    public IEnumerator<CommitInfo> GetEnumerator() => Commits.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}