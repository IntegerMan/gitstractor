using GitStractor.GitObservers;
using GitStractor.Model;
using LibGit2Sharp;

namespace GitStractor.CLI;

/// <summary>
/// This program is the command line utility to extract git repository statistics.
///
/// The program is intended to be invoked either using just "gitstract" to analyze the current directory (or its parent
/// git directory) or via "gitstract C:\some\other\repository" to analyze another repository on disk.
/// </summary>
public static class Program {
    private static void Main(string[] args) {
        // First parameter is path, but current directory is used in its absence
        string repositoryPath = args.FirstOrDefault() ?? Environment.CurrentDirectory;

        Console.WriteLine($"Analyzing git repository in '{repositoryPath}'...{Environment.NewLine}");

        // For now, let's just always dump into the current directory
        string outputDirectory = Environment.CurrentDirectory;

        List<IGitObserver> observers = new() {
                new GitAuthorObserver(),
                new GitCommitObserver(),
                new FileObserver(),
                new FileCommitObserver(),
            };

        // Analyze the git repository
        try {
            GitDataExtractor extractor = new(log: null, observers, treeWalker: new GitTreeWalker(log: null));

            extractor.ExtractInformation(repositoryPath, outputDirectory);

            Console.WriteLine($"Analysis complete");
        }
        catch (RepositoryNotFoundException) {
            Console.WriteLine($"The repository at {repositoryPath} could not be found.");
        }
        catch (IOException ex) {
            Console.WriteLine($"A storage-related error occurred while extracting information: {ex.Message}");
        }
    }

}