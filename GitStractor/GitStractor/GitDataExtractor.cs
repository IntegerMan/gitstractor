using GitStractor.Model;
using GitStractor.Utilities;

namespace GitStractor;

/// <summary>
/// This class is the main entry point for using GitDataExtractor to extract information from a Git repository.
/// </summary>
public class GitDataExtractor
{
    private readonly Dictionary<string, AuthorInfo> _authors = new();

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
    public IEnumerable<CommitInfo> ExtractCommitInformation(GitExtractionOptions options)
    {
        // Validation
        if (options == null) throw new ArgumentNullException(nameof(options));

        _authors.Clear();

        // Analyze the git repository
        string gitPath = FileUtilities.GetParentGitDirectory(options.RepositoryPath);
        using Repository repo = new(gitPath);

        // Create or overwrite the commit output file
        string commitCsvFile = Path.Combine(options.OutputDirectory, options.CommitFilePath);
        using StreamWriter commitsCsv = new(commitCsvFile, append: false);

        // Create or overwrite the author output file
        string authorCsvFile = Path.Combine(options.OutputDirectory, options.AuthorsFilePath);
        using StreamWriter authorCsv = new(authorCsvFile, append: false);

        // Write the header rows
        authorCsv.WriteLine("Name,Email,NumCommits,TotalBytes");
        commitsCsv.WriteLine("CommitHash,AuthorName,AuthorEmail,AuthorDateUTC,CommitterName,CommitterEmail,CommitterDate,Message,NumFiles,TotalBytes,FileNames");

        // Write all commits
        foreach (Commit commit in repo.Commits)
        {

            // Walk the commit tree to get file information
            ulong bytes = 0;
            List<string> files = new();
            foreach (TreeEntry file in commit.Tree)
            {
                switch (file.TargetType)
                {
                    case TreeEntryTargetType.Blob:
                        Blob blob = (Blob) file.Target;
                        bytes += (ulong) blob.Size;
                        break;
                    case TreeEntryTargetType.GitLink:
                        GitLink gitLink = (GitLink)file.Target;
                        // TODO: Should we do anything with this?
                        break;
                    case TreeEntryTargetType.Tree:
                        // TODO: go recursive?
                        break;
                }

                files.Add(file.Path);
            }

            // Identify author
            AuthorInfo author = GetOrCreateAuthor(commit.Author, bytes, true);
            AuthorInfo committer = GetOrCreateAuthor(commit.Author, 0, false);

            // Create the commit summary info.
            CommitInfo info = CreateCommitFromLibGitCommit(files, commit, author, committer, bytes);

            // Write to the CSV file, protecting against commas in various fields
            WriteCommit(commitsCsv, info);

            yield return info;
        }

        // Write the authors to disk
        foreach (AuthorInfo author in _authors.Values)
        {
            authorCsv.WriteLine($"{author.Name},{author.Email},{author.NumCommits},{author.TotalSizeInBytes}");
        }
    }

    private AuthorInfo GetOrCreateAuthor(Signature signature, ulong bytes, bool isAuthor)
    {
        string key = signature.Email.ToLowerInvariant();
        if (!_authors.TryGetValue(key, out AuthorInfo? author))
        {
            author = new AuthorInfo()
            {
                Email = signature.Email,
                Name = signature.Name,
                EarliestCommitDateUtc = signature.When.UtcDateTime,
                LatestCommitDateUtc = signature.When.UtcDateTime,
                NumCommits = isAuthor ? 1 : 0,
                TotalSizeInBytes = bytes
            };
            _authors.Add(key, author);
        }
        else
        {
            author.NumCommits += isAuthor ? 1 : 0;
            author.TotalSizeInBytes += bytes;

            if (signature.When.UtcDateTime > author.LatestCommitDateUtc)
            {
                author.LatestCommitDateUtc = signature.When.UtcDateTime;
            }
            if (signature.When.UtcDateTime < author.EarliestCommitDateUtc)
            {
                author.EarliestCommitDateUtc = signature.When.UtcDateTime;
            }
        }

        return author;
    }

    private static void WriteCommit(TextWriter writer, CommitInfo info)
    {
        writer.Write($"{info.Sha},");
        writer.Write($"{info.AuthorName.ToCsvSafeString()},{info.AuthorEmail.ToCsvSafeString()},{info.AuthorDateUtc},");
        writer.Write($"{info.CommitterName.ToCsvSafeString()},{info.CommitterEmail.ToCsvSafeString()},{info.CommitterDateUtc},");
        writer.Write($"{info.Message.ToCsvSafeString()},");
        writer.WriteLine($"{info.NumFiles},{info.SizeInBytes},{info.FileNames.ToCsvSafeString()}");
    }

    private static CommitInfo CreateCommitFromLibGitCommit(IEnumerable<string> files, 
        Commit commit, 
        AuthorInfo author,
        AuthorInfo committer, 
        ulong bytes) 
        => new(files)
        {
            Sha = commit.Sha,
            Message = commit.MessageShort, // This is just the first line of the commit message. Usually all that's needed
            SizeInBytes = bytes,

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