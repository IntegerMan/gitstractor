namespace GitStractor;

/// <summary>
/// This class represents all options that GitStractor needs
/// to analyze a git repository.
/// </summary>
public record GitExtractionOptions
{
    /// <summary>
    /// The path to the repository to analyze.
    /// This should be a local path to a folder on disk.
    /// </summary>
    public required string RepositoryPath { get; init; }

    /// <summary>
    /// The directory output files should be written to. This folder should already exist.
    /// This defaults to the current directory.
    /// </summary>
    public string OutputDirectory { get; init; } = Environment.CurrentDirectory;
    
    /// <summary>
    /// The path that commit data will be written to in CSV format.
    /// This defaults to <c>Commits.csv</c> in the output directory.
    /// </summary>
    public string CommitFilePath { get; init; } = "Commits.csv";
}