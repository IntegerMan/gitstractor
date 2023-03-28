using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitStractor.Model;

namespace GitStractor.Writers;

public class AuthorInMemoryDataWriter : AuthorDataWriter, IEnumerable<AuthorInfo>
{
    private readonly List<AuthorInfo> _authors = new();

    protected override void Dispose(bool disposing)
    {
        // Nothing to dispose
    }

    public override void BeginWriting() => _authors.Clear();

    public override void WriteAuthors(IEnumerable<AuthorInfo> authors) => _authors.AddRange(authors);

    public IEnumerable<AuthorInfo> Authors => _authors.AsReadOnly();
    public IEnumerator<AuthorInfo> GetEnumerator() => Authors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}