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
    public ulong Bytes { get; init; }
    public string Commit { get; init; }
    public DateTime CreatedDateUtc { get; init; }

    public override string ToString() 
        => $"{Sha[..6]} {Path} @ {Commit[..6]} ({State})";

    public RepositoryFileInfo AsFinalVersion() =>
        new()
        {
            State = FileState.Final,
            Bytes = this.Bytes,
            Commit = this.Commit,
            Name = this.Name,
            Path = this.Path,
            Sha = this.Sha,
            CreatedDateUtc = this.CreatedDateUtc,
        };
}