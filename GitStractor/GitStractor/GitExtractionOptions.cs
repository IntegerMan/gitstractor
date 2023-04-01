using GitStractor.Model;
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
    
    public static GitExtractionOptions BuildConsoleOptions(string repositoryPath) 
        => new()
        {
            RepositoryPath = repositoryPath,
            AuthorWriter = new AuthorConsoleDataWriter(),
            CommitWriter = new CommitConsoleDataWriter(),
            FileWriter = new FileConsoleDataWriter()
        };    
        
    public static GitExtractionOptions BuildInMemoryOptions(string repositoryPath) 
        => new()
        {
            RepositoryPath = repositoryPath,
            AuthorWriter = new AuthorInMemoryDataWriter(),
            CommitWriter = new CommitInMemoryDataWriter(),
            FileWriter = new FileInMemoryDataWriter()
        };    
    
    public static GitExtractionOptions BuildFileOptions(string repositoryPath, string? outputDirectory = null)
    {
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            outputDirectory = Environment.CurrentDirectory;
        }
        
        return new GitExtractionOptions
        {
            RepositoryPath = repositoryPath,
            AuthorWriter = new AuthorCsvDataWriter(Path.Combine(outputDirectory, "Authors.csv")),
            CommitWriter = new CommitCsvDataWriter(Path.Combine(outputDirectory, "Commits.csv")),
            FileWriter = new FileCompoundDataWriter(new FileDataWriter[] {
                new FileCsvDataWriter(Path.Combine(outputDirectory, "Files.csv"), FileState.Added | FileState.Deleted | FileState.Modified),
                new FileCsvDataWriter(Path.Combine(outputDirectory, "FinalStructure.csv"), FileState.Final),
            })
        };
    }
}