using LibGit2Sharp;

namespace GitStractor.CLI;

public class Program
{
    private static void Main(string[] args)
    {
        string path = "C:\\Dev\\GitDataExtractor";
        string outputPath = Path.Combine(Environment.CurrentDirectory, "output.csv");

        try
        {
            GitDataExtractor.ExtractCommitInformation(path, outputPath);
        }
        catch (RepositoryNotFoundException)
        {
            Console.WriteLine($"The repository at {path} could not be found.");
        }
    }
}