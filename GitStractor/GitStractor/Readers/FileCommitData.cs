namespace GitStractor.Readers;

public record FileCommitData
{
    public required string FilePath { get; set; }
    public required string FileHash { get; set; }
    public required string CommitHash { get; set; }
    public required string AuthorEmail { get; set; }
    public required string AuthorDateUTC { get; set; }
    public required string CommitterEmail { get; set; }
    public required string CommitterDateUTC { get; set; }
    public required string Message { get; set; }
    public required int Bytes { get; set; }
    public required int Lines { get; set; }
    public required int NetLines { get; set; }
}
