namespace GitStractor.Model;

public class RepositoryFileInfo
{
    public required string Name { get; set; }
    public required string Sha { get; init; }
    public required string Path { get; set; }
    public ulong Bytes { get; init; }
    public string Commit { get; init; }
    public DateTime CreatedDateUtc { get; init; }
}