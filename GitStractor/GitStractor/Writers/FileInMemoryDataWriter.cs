using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitStractor.Model;

namespace GitStractor.Writers;

public class FileInMemoryDataWriter : FileDataWriter, IEnumerable<RepositoryFileInfo>
{
    private readonly List<RepositoryFileInfo> _files = new();

    protected override void Dispose(bool disposing)
    {
        // Nothing to dispose
    }

    public override void BeginWriting() => _files.Clear();

    public override void WriteFile(RepositoryFileInfo file) => _files.Add(file);

    public IEnumerable<RepositoryFileInfo> Files => _files.AsReadOnly();
    public IEnumerator<RepositoryFileInfo> GetEnumerator() => Files.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}