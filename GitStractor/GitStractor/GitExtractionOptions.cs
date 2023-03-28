using GitStractor.Writers;

namespace GitStractor;

/// <summary>
/// This class represents all options that GitStractor needs
/// to analyze a git repository.
/// </summary>
public class GitExtractionOptions
{
    /// <summary>
    /// The path to the repository to analyze.
    /// This should be a local path to a folder on disk.
    /// </summary>
    public required string RepositoryPath { get; init; }

    public required AuthorDataWriter AuthorWriter { get; init; }
    public required CommitDataWriter CommitWriter { get; init; }
}