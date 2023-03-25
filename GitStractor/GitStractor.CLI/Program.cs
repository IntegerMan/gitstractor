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
            RepositoryPath = repositoryPath  
        };
        
        // Analyze the git repository
        try
        {
            GitDataExtractor.ExtractCommitInformation(options);
        }
        catch (RepositoryNotFoundException)
        {
            Console.WriteLine($"The repository at {repositoryPath} could not be found.");
        }
    }
}