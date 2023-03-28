using GitStractor.Model;
using GitStractor.Utilities;

namespace GitStractor;

/// <summary>
/// This class is the main entry point for using GitDataExtractor to extract information from a Git repository.
/// </summary>
public class GitDataExtractor : IDisposable
{
    private readonly GitExtractionOptions _options;
    private readonly Dictionary<string, AuthorInfo> _authors = new();

    /// <summary>
    /// Creates a new instance of the <see cref="GitDataExtractor"/> class.
    /// </summary>
    /// <param name="options">The extraction options governing the git analysis</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> is <c>null</c>.
    /// </exception>
    public GitDataExtractor(GitExtractionOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Extracts commit, author, and file information from the git repository.
    /// </summary>
    /// <exception cref="RepositoryNotFoundException">
    /// Thrown when the repository does not exist
    /// </exception>
    public void ExtractInformation()
    {
        _authors.Clear();

        // Analyze the git repository
        string gitPath = FileUtilities.GetParentGitDirectory(_options.RepositoryPath);
        using Repository repo = new(gitPath);

        // Create or overwrite the commit output file
        string commitCsvFile = Path.Combine(_options.OutputDirectory, _options.CommitFilePath);
        using StreamWriter commitsCsv = new(commitCsvFile, append: false);

        // Write the header rows
        _options.AuthorWriter.BeginWriting();
        commitsCsv.WriteLine("CommitHash,AuthorEmail,AuthorDateUTC,CommitterEmail,CommitterDate,Message,NumFiles,TotalBytes,FileNames");

        // Write all commits
        foreach (Commit commit in repo.Commits)
        {
            // Walk the commit tree to get file information
            ulong bytes = 0;
            List<string> files = new();
            foreach (TreeEntry file in commit.Tree)
            {
                if (file.TargetType == TreeEntryTargetType.Blob) 
                { 
                    Blob blob = (Blob) file.Target;
                    bytes += (ulong) blob.Size;
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
        }

        // Write the authors to their destination
        _options.AuthorWriter.WriteAuthors(_authors.Values);
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
        writer.Write($"{info.Author.Email.ToCsvSafeString()},{info.AuthorDateUtc},");
        writer.Write($"{info.Committer.Email.ToCsvSafeString()},{info.CommitterDateUtc},");
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
            Author = author,
            AuthorDateUtc = commit.Author.When.UtcDateTime,
                    
            // Committer information. Committer is the person who performed the commit
            Committer = committer,
            CommitterDateUtc = commit.Committer.When.UtcDateTime,
        };

    public void Dispose()
    {
        _options.AuthorWriter.Dispose();
    }
}