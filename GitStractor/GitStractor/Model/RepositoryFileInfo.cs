namespace GitStractor.Model;

public class RepositoryFileInfo
{
    public FileState State { get; set; } = FileState.Unmodified;
    public required string Name { get; set; }
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
    public string? Path1 => GetPathElement(0);
    public string? Path2 => GetPathElement(1);
    public string? Path3 => GetPathElement(2);
    public string? Path4 => GetPathElement(3);
    public string? Path5 => GetPathElement(4);

    private string? GetPathElement(int index)
    {
        string[] paths = Path.Split("/");
        
        // +1 here is because the filename itself is part of the path
        return paths.Length > index + 1 ? paths[index] : null;
    }

    public ulong Bytes { get; init; }
    public string Commit { get; init; }
    public DateTime CreatedDateUtc { get; init; }
    public required int Lines { get; init; }

    public override string ToString() 
        => $"{Sha[..6]} {Path} @ {Commit[..6]} ({State})";

    public RepositoryFileInfo AsFinalVersion() 
        => new()
        {
            State = FileState.Final,
            Bytes = this.Bytes,
            Lines = this.Lines,
            Commit = this.Commit,
            Name = this.Name,
            Path = this.Path,
            Sha = this.Sha,
            CreatedDateUtc = this.CreatedDateUtc,
        };

    public RepositoryFileInfo AsDeletedFile(string commit, DateTime commitDateUtc)
        => new()
        {
            State = FileState.Deleted,
            Bytes = 0, // TODO: Should this be the size of the file at the time of deletion?
            Lines = 0,
            Commit = commit,
            Name = this.Name,
            Path = this.Path,
            Sha = this.Sha, // May want to use string.empty here, but leaving for comparison of same file over time
            CreatedDateUtc = commitDateUtc,
        };
}