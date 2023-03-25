namespace GitStractor;

/// <summary>
/// This class is the main entry point for using GitDataExtractor to extract information from a Git repository.
/// </summary>
public static class GitDataExtractor
{
    /// <summary>
    /// Extracts commit information into an output file that can be analyzed by other tools.
    /// </summary>
    /// <exception cref="RepositoryNotFoundException">
    /// Thrown when the repository listed in <paramref name="options"/> does not exist
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> is <c>null</c>.
    /// </exception>
    /// <param name="options">The configuration options for GitStractor</param>
    public static void ExtractCommitInformation(GitExtractionOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        string gitPath = FileUtilities.GetParentGitDirectory(options.RepositoryPath);

        using Repository repo = new(gitPath);
        
        foreach (Commit c in repo.Commits)
        {
            Console.WriteLine($"{c.Sha[..4]}: {c.Author.Name} ({c.Author.Email}): {c.MessageShort.Trim()}");
        }
    }
}