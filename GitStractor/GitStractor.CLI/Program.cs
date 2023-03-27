using GitStractor.Model;
using LibGit2Sharp;

namespace GitStractor.CLI;

public class Program
{
    private static void Main(string[] args)
    {
        // First parameter is path, but current directory is used in its absence
        string? repositoryPath = args.FirstOrDefault() ?? Environment.CurrentDirectory;

        GitExtractionOptions options = new()
        {
            RepositoryPath = repositoryPath,
            OutputDirectory = Environment.CurrentDirectory,
            CommitFilePath = "Commits.csv"
        };
        
        // Analyze the git repository
        try
        {
            GitDataExtractor extractor = new();
            foreach (CommitInfo commitInfo in extractor.ExtractCommitInformation(options))
            {
                Console.WriteLine(commitInfo);
            }
        }
        catch (RepositoryNotFoundException)
        {
            Console.WriteLine($"The repository at {repositoryPath} could not be found.");
        }
    }
}