using GitStractor.Model;
using GitStractor.Utilities;

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
        // Validation
        if (options == null) throw new ArgumentNullException(nameof(options));

        // Analyze the git repository
        string gitPath = FileUtilities.GetParentGitDirectory(options.RepositoryPath);
        using Repository repo = new(gitPath);

        // Create or overwrite the commit output file
        string commitCSVFile = Path.Combine(options.OutputDirectory, options.CommitFilePath);
        using StreamWriter commitsCsv = new(commitCSVFile, append: false);
       
        // Write the header row
        commitsCsv.WriteLine("CommitHash,AuthorName,AuthorEmail,AuthorDateUTC,CommitterName,CommitterEmail,CommitterDate,Message,NumFiles,FileNames");

        // Write all commits
        foreach (Commit commit in repo.Commits)
        {
            // Walk the commit tree to get file information
            List<string> files = new();
            foreach (TreeEntry file in commit.Tree)
            {
                files.Add(file.Path);
            }
                
            // Create the commit summary info.
            CommitInfo info = CreateCommitFromLibGitCommit(files, commit);

            // Write to the CSV file, protecting against commas in various fields
            WriteCommit(commitsCsv, info);

            yield return info;
        }
    }

    private static void WriteCommit(TextWriter writer, CommitInfo info)
    {
        writer.Write($"{info.Sha},");
        writer.Write($"{info.AuthorName.ToCsvSafeString()},{info.AuthorEmail.ToCsvSafeString()},{info.AuthorDateUtc},");
        writer.Write($"{info.CommitterName.ToCsvSafeString()},{info.CommitterEmail.ToCsvSafeString()},{info.CommitterDateUtc},");
        writer.Write($"{info.Message.ToCsvSafeString()},");
        writer.WriteLine($"{info.NumFiles},{info.FileNames.ToCsvSafeString()}");
    }

    private static CommitInfo CreateCommitFromLibGitCommit(IEnumerable<string> files, Commit commit) 
        => new(files)
        {
            Sha = commit.Sha,
            Message = commit.MessageShort, // This is just the first line of the commit message. Usually all that's needed

            // Author information. Author is the person who wrote the contents of the commit
            AuthorName = commit.Author.Name,
            AuthorEmail = commit.Author.Email,
            AuthorDateUtc = commit.Author.When.UtcDateTime,
                    
            // Committer information. Committer is the person who performed the commit
            CommitterName = commit.Committer.Name,
            CommitterEmail = commit.Committer.Email,
            CommitterDateUtc = commit.Committer.When.UtcDateTime,
        };
}