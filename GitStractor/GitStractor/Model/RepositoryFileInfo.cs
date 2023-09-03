namespace GitStractor.Model;

public class RepositoryFileInfo
{
    public FileState State { get; set; } = FileState.Unmodified;
    public required string Sha { get; init; }
    public required string Path { get; set; }

    public string Extension
    {
        get
        {
            FileInfo info = new(Path);
            return info.Extension;
        }
    }

    public required string Commit { get; init; }
    public DateTime CreatedDateUtc { get; init; }
    public required int Lines { get; init; }
    public required int LinesDelta { get; init; }
    public string? OldPath { get; init; }
    public int LinesDeleted { get; internal set; }
    public int LinesAdded { get; internal set; }

    public override string ToString() 
        => $"{Sha[..6]} {Path} @ {Commit[..6]} ({State})";
}