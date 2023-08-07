using GitStractor.Model;
using GitStractor.Utilities;

namespace GitStractor;

/// <summary>
/// This class is the main entry point for using GitDataExtractor to extract information from a Git repository.
/// </summary>
public class GitDataExtractor : IDisposable {
    private GitExtractionOptions? _options;
    private readonly Dictionary<string, AuthorInfo> _authors = new();
    private readonly Dictionary<string, GitTreeInfo> _trees = new();

    /// <summary>
    /// Extracts commit, author, and file information from the git repository.
    /// </summary>
    /// <param name="options">The options to use when extracting information</param>
    /// <exception cref="RepositoryNotFoundException">
    /// Thrown when the repository does not exist
    /// </exception>
    public void ExtractInformation(GitExtractionOptions options) {
        _options = options;

        // Clear old state
        _authors.Clear();
        _pathShas.Clear();
        _trees.Clear();

        // If we got a git directory that isn't actually a git directory, look for a .git file in its parents
        string? gitPath = FileUtilities.GetParentGitDirectory(_options.RepositoryPath);

        // If we didn't find a git directory, throw an exception
        if (gitPath == null) {
            throw new RepositoryNotFoundException($"Could not find a git repository at {_options.RepositoryPath}");
        }

        _options.ProgressListener?.Started($"Analyzing {gitPath}");

        // Connect to the git repository
        var repoOptions = new RepositoryOptions() {
            Identity = new Identity("Test", "Testerson@gmail.com"),            
        };
        try {
            using Repository repo = new(gitPath, repoOptions);

            // Write the header rows
            _options.AuthorWriter.BeginWriting();
            _options.CommitWriter.BeginWriting();
            _options.FileWriter.BeginWriting();

            // Customize how we'll traverse commit history
            CommitFilter filter = new() {
                // It's very important to walk through commits in historical order from oldest to newest
                // This lets us accurately get a read on the state of the repository at the time of each commit
                SortBy = CommitSortStrategies.Reverse,

                // We only want to look at commits that are reachable from whatever is HEAD at the moment
                IncludeReachableFrom = repo.Head
            };

            double totalCommits = SearchCommits(repo, filter).Count();

            // Loop over each commit
            int commitNum = 0;
            foreach (Commit commit in SearchCommits(repo, filter)) {
                commitNum++;
                _options.ProgressListener?.UpdateProgress(commitNum / totalCommits, $"Analyzing commit {commitNum} of {totalCommits}");
                bool isLast = commit == repo.Head.Tip;
                ProcessCommit(commit, isLast);
            }

            // Write all authors at the end, now that we know aggregate-level information
            _options.AuthorWriter.WriteAuthors(_authors.Values);

            _options.ProgressListener?.Completed();
        }
        catch (LibGit2SharpException ex) {
            if (ex.Message.Contains("is not owned by current user", StringComparison.OrdinalIgnoreCase)) {
                throw new InvalidOperationException("GitStractor requires the directory to be marked as safe. You can mark all directories as safe globally via the following git command: git config --global --add safe.directory \"*\"", ex);
            }

            throw;
        }
    }

    private ICommitLog SearchCommits(Repository repo, CommitFilter filter) => repo.Commits.QueryBy(filter);

    private void ProcessCommit(Commit commit, bool isLast) {
        GitTreeInfo treeInfo = new();
        _trees[commit.Tree.Sha] = treeInfo;

        // Walk the commit tree to get file information
        WalkTree(commit, commit.Tree, treeInfo, isLast);

        // Get what we know about the parent commit this came from
        GitTreeInfo? parentTree = commit.Parents.Any() ? _trees[commit.Parents.First().Tree.Sha] : null;

        // Detect any Deleted Files
        if (parentTree != null) {
            foreach (string path in parentTree.Files) {
                if (!treeInfo.Contains(path)) {
                    RepositoryFileInfo file = parentTree.Find(path)!;
                    _options!.FileWriter.WriteFile(file.AsDeletedFile(commit.Sha, commit.Author.When.UtcDateTime));
                }
            }
        }

        // Identify author
        AuthorInfo author = GetOrCreateAuthor(commit.Author, treeInfo.Bytes, true);
        AuthorInfo committer = GetOrCreateAuthor(commit.Author, 0, false);

        // Create the commit summary info.
        CommitInfo info = CreateCommitFromLibGitCommit(commit, author, committer, treeInfo);

        // Write the commit to the appropriate writer
        _options!.CommitWriter.Write(info);
    }

    private readonly Dictionary<string, string> _pathShas = new();
    private readonly Dictionary<string, RepositoryFileInfo> _fileInfo = new();

    private void WalkTree(Commit commit, Tree tree, GitTreeInfo treeInfo, bool isLast) {
        foreach (TreeEntry treeEntry in tree) {
            if (treeEntry.TargetType == TreeEntryTargetType.Blob) {
                // Ignore the file if it's not an extension we care about
                FileInfo info = new(treeEntry.Path);
                if (_options!.FileMatchesFilter(info.Extension)) {
                    _fileInfo.TryGetValue(treeEntry.Path, out RepositoryFileInfo? oldInfo);

                    RepositoryFileInfo fileInfo;
                    if (oldInfo != null && treeEntry.Target.Sha == oldInfo.Sha) {
                        fileInfo = oldInfo.AsUnchanged();
                    } else {
                        fileInfo = BuildFileInfo(commit, treeEntry, oldInfo);
                    }
                    _fileInfo[treeEntry.Path] = fileInfo;

                    // Each tree consists of ALL files, including those who didn't change at all. When the contents of a
                    // file changes, its SHA changes. So, we can use the SHA of the file to determine if it changed.
                    // If the SHA is the same, we shouldn't log the file as being part of this commit, though there may
                    // be analysis value of having a full log of all files as of any given commit in the system
                    string fileLower = treeEntry.Path.ToLowerInvariant();
                    fileInfo.State = DetermineFileState(fileLower, treeEntry);

                    treeInfo.Register(fileInfo);

                    // Add or update our entry for the file's path
                    _pathShas[fileLower] = treeEntry.Target.Sha;

                    _options.FileWriter.WriteFile(fileInfo);
                    // At the very end of the analysis, we want to write out the final state of the file
                    if (isLast) {
                        _options.FileWriter.WriteFile(fileInfo.AsFinalVersion());
                    }
                }
            } else if (treeEntry.TargetType == TreeEntryTargetType.Tree) {
                Tree subTree = (Tree)treeEntry.Target;

                WalkTree(commit, subTree, treeInfo, isLast);
            }
        }
    }

    private FileState DetermineFileState(string fileLower, TreeEntry treeEntry) {
        if (!_pathShas.ContainsKey(fileLower)) {
            // If we didn't have the path before, this is an added file so let's mark it as an added file
            return FileState.Added;
        }

        return _pathShas[fileLower] == treeEntry.Target.Sha
            ? FileState.Unmodified
            : FileState.Modified;
    }

    private static RepositoryFileInfo BuildFileInfo(Commit commit, TreeEntry treeEntry, RepositoryFileInfo? oldInfo) {
        Blob blob = (Blob)treeEntry.Target;

        int lines = CountLinesInFile(blob);

        int linesDelta = lines - oldInfo?.Lines ?? lines;

        RepositoryFileInfo fileInfo = new() {
            Name = treeEntry.Name,
            Path = treeEntry.Path,
            Sha = blob.Id.Sha,
            Lines = lines,
            LinesDelta = linesDelta,
            Bytes = (ulong)blob.Size,
            Commit = commit.Id.Sha,
            CreatedDateUtc = commit.Author.When.UtcDateTime,
        };
        return fileInfo;
    }

    private static int CountLinesInFile(Blob blob) {
        int lines = 0;
        using var reader = new StreamReader(blob.GetContentStream());

        while (!reader.EndOfStream) {
            _ = reader.ReadLine();
            lines++;
        }

        return lines;
    }

    private AuthorInfo GetOrCreateAuthor(Signature signature, ulong bytes, bool isAuthor) {
        string key = signature.Email.ToLowerInvariant();
        if (!_authors.TryGetValue(key, out AuthorInfo? author)) {
            author = new AuthorInfo() {
                Email = signature.Email,
                Name = signature.Name,
                EarliestCommitDateUtc = signature.When.UtcDateTime,
                LatestCommitDateUtc = signature.When.UtcDateTime,
                NumCommits = isAuthor ? 1 : 0,
                TotalSizeInBytes = bytes
            };
            _authors.Add(key, author);
        } else {
            author.NumCommits += isAuthor ? 1 : 0;
            author.TotalSizeInBytes += bytes;

            if (signature.When.UtcDateTime > author.LatestCommitDateUtc) {
                author.LatestCommitDateUtc = signature.When.UtcDateTime;
            }

            if (signature.When.UtcDateTime < author.EarliestCommitDateUtc) {
                author.EarliestCommitDateUtc = signature.When.UtcDateTime;
            }
        }

        return author;
    }

    private static CommitInfo CreateCommitFromLibGitCommit(
        Commit commit,
        AuthorInfo author,
        AuthorInfo committer,
        GitTreeInfo treeInfo)
        => new(treeInfo.ModifiedFiles) {
            Sha = commit.Sha,
            Message = commit
                .MessageShort, // This is just the first line of the commit message. Usually all that's needed
            SizeInBytes = treeInfo.Bytes,
            TotalFiles = treeInfo.TotalFileCount,
            AddedFiles = treeInfo.AddedFileCount,
            DeletedFiles = treeInfo.DeletedFileCount,

            // Author information. Author is the person who wrote the contents of the commit
            Author = author,
            AuthorDateUtc = commit.Author.When.UtcDateTime,

            // Committer information. Committer is the person who performed the commit
            Committer = committer,
            CommitterDateUtc = commit.Committer.When.UtcDateTime,
        };

    public void Dispose() {
        if (_options != null) {
            _options.AuthorWriter.Dispose();
            _options.CommitWriter.Dispose();
            _options.FileWriter.Dispose();
            _options = null;
        }
    }
}