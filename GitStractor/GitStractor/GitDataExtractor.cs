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
        _pathShas.Clear();

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
        CommitFilter filter = new CommitFilter()
        {
            SortBy = CommitSortStrategies.Reverse,
            IncludeReachableFrom = repo.Head
        };
        foreach (Commit commit in repo.Commits.QueryBy(filter))
        {
            ProcessCommit(commit);
        }
        
        // Write all authors at the end, now that we know aggregate-level information
        _options.AuthorWriter.WriteAuthors(_authors.Values);
    }

    private void ProcessCommit(Commit commit)
    {
        GitTreeInfo treeInfo = new();
        List<string> files = new();

        // Walk the commit tree to get file information
        WalkTree(commit, commit.Tree, treeInfo, files);

        // Identify author
        AuthorInfo author = GetOrCreateAuthor(commit.Author, treeInfo.Bytes, true);
        AuthorInfo committer = GetOrCreateAuthor(commit.Author, 0, false);

        // Create the commit summary info.
        CommitInfo info = CreateCommitFromLibGitCommit(files, commit, author, committer, treeInfo.Bytes);

        // Write the commit to the appropriate writer
        _options.CommitWriter.Write(info);
    }

    private readonly Dictionary<string, string> _pathShas = new();

    private void WalkTree(Commit commit, Tree tree, GitTreeInfo treeInfo, ICollection<string> files)
    {
        foreach (TreeEntry treeEntry in tree)
        {
            if (treeEntry.TargetType == TreeEntryTargetType.Blob)
            {
                // Each tree consists of ALL files, including those who didn't change at all. When the contents of a
                // file changes, its SHA changes. So, we can use the SHA of the file to determine if it changed.
                // If the SHA is the same, we shouldn't log the file as being part of this commit, though there may
                // be analysis value of having a full log of all files as of any given commit in the system
                string fileLowerSha = treeEntry.Path.ToLowerInvariant();
                if (_pathShas.ContainsKey(treeEntry.Target.Sha) && _pathShas[treeEntry.Target.Sha] == fileLowerSha)
                {
                    continue;
                }
                
                // Add or update our entry for the file's path
                _pathShas[treeEntry.Target.Sha] = fileLowerSha;
                
                Blob blob = (Blob)treeEntry.Target;

                RepositoryFileInfo fileInfo = new()
                {
                    Name = treeEntry.Name,
                    Path = treeEntry.Path,
                    Sha = blob.Id.Sha,
                    Bytes = (ulong)blob.Size,
                    Commit = commit.Id.Sha,
                    CreatedDateUtc = commit.Author.When.UtcDateTime,
                };

                treeInfo.Bytes += fileInfo.Bytes;

                _options.FileWriter.WriteFile(fileInfo);
                
                files.Add(treeEntry.Path);
            }
            else if (treeEntry.TargetType == TreeEntryTargetType.Tree)
            {
                Tree subTree = (Tree)treeEntry.Target;
                
                WalkTree(commit, subTree, treeInfo, files);
            }
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