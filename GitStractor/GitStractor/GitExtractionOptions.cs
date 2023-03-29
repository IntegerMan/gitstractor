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

    /// <summary>
    /// The <see cref="DataWriterBase"/> used to save author information when processessing commits.
    /// </summary>
    public required AuthorDataWriter AuthorWriter { get; init; }

    /// <summary>
    /// The <see cref="DataWriterBase"/> used to save commit information when processessing commits.
    /// </summary>
    public required CommitDataWriter CommitWriter { get; init; }

    /// <summary>
    /// The <see cref="DataWriterBase"/> used to save information on individual files when processessing commits.
    /// </summary>
    public required FileDataWriter FileWriter { get; init; }
}