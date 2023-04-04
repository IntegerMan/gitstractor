namespace GitStractor.Readers;

/// <summary>
/// Represents information about a commit that has been loaded up from a data file after extraction
/// </summary>
public record CommitData
{
    public required string CommitHash { get; init; }
    public required string AuthorEmail { get; init; }
    public required DateTime AuthorDateUTC { get; init; }
    public required string CommitterEmail { get; init; }
    public required DateTime CommitterDateUTC { get; init; }
    public required string Message {get; init; }
    public required int NumFiles {get; init;}
    public required int AddedFiles {get; init;}
    public required int DeletedFiles {get; init;}
    public required int TotalFiles {get; init;}
    public required int TotalBytes {get; init;}
    public required string FileNames {get; init;}
    public required int TotalLines {get; init;}
    public required int NetLines {get; init;}
}