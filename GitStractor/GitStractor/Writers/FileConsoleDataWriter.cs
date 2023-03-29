using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitStractor.Model;

namespace GitStractor.Writers;

public class FileConsoleDataWriter : FileDataWriter
{
    protected override void Dispose(bool disposing)
    {
        // Nothing to dispose
    }

    public override void WriteFile(RepositoryFileInfo file)
    {
        Console.WriteLine(file);
    }
}