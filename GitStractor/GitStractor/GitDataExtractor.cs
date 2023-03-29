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

        // If we got a git directory that isn't actually a git directory, look for a .git file in its parents
        string? gitPath = FileUtilities.GetParentGitDirectory(_options.RepositoryPath);

        // If we didn't find a git directory, throw an exception
        if (gitPath == null)
        {
            throw new RepositoryNotFoundException($"Could not find a git repository at {_options.RepositoryPath}");
        }
        
        // Connect to the git repository
        using Repository repo = new(gitPath);

        // Write the header rows
        _options.AuthorWriter.BeginWriting();
        _options.CommitWriter.BeginWriting();

        // Write all commits
        foreach (Commit commit in repo.Commits)
        {
            ProcessCommit(commit);
        }
        
        // Write all authors at the end, now that we know aggregate-level information
        _options.AuthorWriter.WriteAuthors(_authors.Values);
    }

    private void ProcessCommit(Commit commit)
    {
        ulong bytes = 0;
        List<string> files = new();

        // Walk the commit tree to get file information
        foreach (TreeEntry treeEntry in commit.Tree)
        {
            if (treeEntry.TargetType == TreeEntryTargetType.Blob)
            {
                Blob blob = (Blob) treeEntry.Target;
                
                RepositoryFileInfo fileInfo = new()
                {
                    Name = treeEntry.Name,
                    Path = treeEntry.Path,
                    Sha = blob.Sha,
                    Bytes = (ulong) blob.Size,
                    Commit = commit.Sha,
                    CreatedDateUtc = commit.Author.When.UtcDateTime,
                };

                bytes += fileInfo.Bytes;

                _options.FileWriter.WriteFile(fileInfo);
            }

            files.Add(treeEntry.Path);
        }

        // Identify author
        AuthorInfo author = GetOrCreateAuthor(commit.Author, bytes, true);
        AuthorInfo committer = GetOrCreateAuthor(commit.Author, 0, false);

        // Create the commit summary info.
        CommitInfo info = CreateCommitFromLibGitCommit(files, commit, author, committer, bytes);

        // Write the commit to the appropriate writer
        _options.CommitWriter.Write(info);
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
        _options.CommitWriter.Dispose();
    }
}