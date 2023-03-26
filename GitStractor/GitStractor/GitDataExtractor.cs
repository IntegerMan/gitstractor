using GitStractor.Model;

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
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="CommitInfo"/> objects representing the commits in the repository.
    /// </returns>
    public static IEnumerable<CommitInfo> ExtractCommitInformation(GitExtractionOptions options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));

        string gitPath = FileUtilities.GetParentGitDirectory(options.RepositoryPath);

        using Repository repo = new(gitPath);

        // Create or overwrite the commit output file
        string commitCSVFile = Path.Combine(options.OutputDirectory, options.CommitFilePath);
        using (StreamWriter writer = new(commitCSVFile, append: false))
        {
            // Write the header row
            writer.WriteLine("CommitHash,AuthorName,AuthorEmail,AuthorDate,CommitterName,CommitterEmail,CommitterDate,Message");

            // Write all commits
            foreach (Commit commit in repo.Commits)
            {
                CommitInfo info = new()
                {
                    Sha = commit.Sha,
                    AuthorName = commit.Author.Name,
                    AuthorEmail = commit.Author.Email,
                    AuthorDate = commit.Author.When.UtcDateTime,
                    Message = commit.MessageShort // This is just the first line of the commit message. Usually all that's needed
                };
                
                writer.WriteLine($"{info.Sha},{info.AuthorName},{info.AuthorEmail},{info.AuthorDate},{info.Message}");

                yield return info;
            }
        }
    }
}