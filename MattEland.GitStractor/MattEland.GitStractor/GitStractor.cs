using LibGit2Sharp;

namespace GitStractor;

/// <summary>
/// This class is the main entry point for using GitStractor to extract information from a Git repository.
/// </summary>
public class GitStractor
{
    /// <summary>
    /// Extracts commit information into an output file that can be analyzed by other tools.
    /// </summary>
    /// <exception cref="RepositoryNotFoundException">
    /// Thrown when the repository in <paramref name="repoPath"/> does not exist
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="repoPath"/> is <c>null</c>.
    /// </exception>
    /// <param name="repoPath">
    /// The path to the git repository.
    /// This should be a path to a local folder on disk.
    /// </param>
    /// <param name="outputPath"></param>
    public void ExtractCommitInformation(string repoPath, string outputPath)
    {
        if (repoPath == null) throw new ArgumentNullException(nameof(repoPath));

        using Repository repo = new(repoPath);
        
        repo.Commits.ToList().ForEach(c =>
        {
            Console.WriteLine($"{c.Sha[..4]}: {c.Author.Name} ({c.Author.Email}): {c.MessageShort.Trim()}");
        });
    }
}