namespace GitStractor.Model;

public class AuthorInfo
{
    public required string Name { get; init; }

    public required string Email { get; init; }

    public int NumCommits { get; set; }
    public int LinesAdded { get; set; }
    public int LinesDeleted { get; set; }

    public required DateTime EarliestCommitDateUtc { get; set; }
    public required DateTime LatestCommitDateUtc { get; set; }
    public bool IsBot { get; set; }
    public int Id { get; set; } = -1;

    public override string ToString()
    {
        return $"{Name} ({Email}): {NumCommits} Commits from {EarliestCommitDateUtc.ToShortDateString()} - {LatestCommitDateUtc.ToShortDateString()}";
    }
}