using GitStractor.Model;
using GitStractor.Writers;
using LibGit2Sharp;

namespace GitStractor.CLI;

public class Program
{
    private static void Main(string[] args)
    {
        // First parameter is path, but current directory is used in its absence
        string repositoryPath = args.FirstOrDefault() ?? Environment.CurrentDirectory;

        //string authorCsvFile = Path.Combine(_options.OutputDirectory, _options.AuthorsFilePath);

        GitExtractionOptions options = new()
        {
            RepositoryPath = repositoryPath,
            OutputDirectory = Environment.CurrentDirectory,
            CommitFilePath = "Commits.csv",
            AuthorWriter = new AuthorCompoundDataWriter(new AuthorDataWriter[] {
                new AuthorConsoleDataWriter(),
                new AuthorCsvDataWriter(Path.Combine(Environment.CurrentDirectory, "Authors.csv")),
            })
        };
        
        // Analyze the git repository
        try
        {
            using GitDataExtractor extractor = new(options);

            foreach (CommitInfo commitInfo in extractor.ExtractCommitInformation())
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