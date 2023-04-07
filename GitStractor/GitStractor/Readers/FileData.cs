namespace GitStractor.Readers;

public record FileData
{
    public required string CommitHash { get; set; }
    public required string FileHash { get; set; }
    public required string Filename { get; set; }
    public required string Extension { get; set; }
    public required string FilePath { get; set; }
    public required string State { get; set; }
    public required int Lines { get; set; }
    public required int Bytes { get; set; }
    public required DateTime CreatedDateUTC { get; set; }
    public required string Path1 { get; set; }
    public required string Path2 { get; set; }
    public required string Path3 { get; set; }
    public required string Path4 { get; set; }
    public required string Path5 { get; set; }
}