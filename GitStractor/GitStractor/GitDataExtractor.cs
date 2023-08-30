using GitStractor.Model;
using GitStractor.Utilities;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using System;

namespace GitStractor;

/// <summary>
/// This class is the main entry point for using GitDataExtractor to extract information from a Git repository.
/// </summary>
public class GitDataExtractor : IDisposable {
    private const string SecurityMessage = "GitStractor requires the directory to be marked as safe. You can mark all directories as safe globally via the following git command: git config --global --add safe.directory \"*\"";
    private readonly Dictionary<string, AuthorInfo> _authors = new();
    private readonly Dictionary<string, GitTreeInfo> _trees = new();

    private ILogger<GitDataExtractor> Log { get; }
    public List<IGitObserver> Observers { get; }

    public GitDataExtractor(ILogger<GitDataExtractor> log, IEnumerable<IGitObserver> observers) {
        Log = log;
        Observers = observers.ToList();
    }

    /// <exception cref="RepositoryNotFoundException">
    /// Thrown when the repository does not exist
    /// </exception>
    public void ExtractInformation(string repoPath, string outputPath) {

        // Clear old state
        _authors.Clear();
        _pathShas.Clear();
        _trees.Clear();

        // If we got a git directory that isn't actually a git directory, look for a .git file in its parents
        string? gitPath = FileUtilities.GetParentGitDirectory(repoPath);

        // If we didn't find a git directory, throw an exception
        if (gitPath == null) {
            string message = $"Could not find a git repository at {repoPath}";
            Log.LogWarning(message);
            throw new RepositoryNotFoundException(message);
        }

        Log.LogInformation($"Analyzing git repository at {gitPath}");

        // Connect to the git repository
        RepositoryOptions repoOptions = new() {
            Identity = new Identity("Test", "Testerson@gmail.com"),            
        };

        try {
            using Repository repo = new(gitPath, repoOptions);

            // Customize how we'll traverse commit history
            CommitFilter filter = new() {
                // It's very important to walk through commits in historical order from oldest to newest
                // This lets us accurately get a read on the state of the repository at the time of each commit
                SortBy = CommitSortStrategies.Reverse,

                // We only want to look at commits that are reachable from whatever is HEAD at the moment
                IncludeReachableFrom = repo.Head
            };

            int totalCommits = SearchCommits(repo, filter).Count();

            Observers.ForEach(o => o.OnBeginningIteration(totalCommits, outputPath));

            // Loop over each commit
            int commitNum = 0;
            foreach (Commit commit in SearchCommits(repo, filter)) {
                commitNum++;
                bool isLast = commit == repo.Head.Tip;
                ProcessCommit(commit, isLast);

                UpdateProgress(totalCommits, commitNum);
            }
            Log.LogInformation($"Enumerated {commitNum} commits");

            // Write all authors at the end, now that we know aggregate-level information
            Observers.ForEach(o => o.OnCompletedIteration(outputPath));

            //_options.ProgressListener?.Completed();
            Log.LogInformation($"Successfully analyzed repository at {repoPath}");
        }
        catch (LibGit2SharpException ex) {

            if (ex.Message.Contains("is not owned by current user", StringComparison.OrdinalIgnoreCase)) {
                Log.LogWarning(SecurityMessage);
                throw new InvalidOperationException("GitStractor requires the directory to be marked as safe. You can mark all directories as safe globally via the following git command: git config --global --add safe.directory \"*\"", ex);
            }

            Log.LogError(ex, ex.Message);

            throw;
        }
    }

    private void UpdateProgress(double totalCommits, int commitNum) {
        double percent = commitNum / totalCommits;
        Observers.ForEach(o => o.UpdateProgress(percent, commitNum, totalCommits));
    }

    private ICommitLog SearchCommits(Repository repo, CommitFilter filter) => repo.Commits.QueryBy(filter);

    private void ProcessCommit(Commit commit, bool isLast) {
        GitTreeInfo treeInfo = new();
        _trees[commit.Tree.Sha] = treeInfo;

        Observers.ForEach(o => o.OnProcessingCommit(commit, isLast));

        // Walk the commit tree to get file information
        WalkTree(commit, commit.Tree, treeInfo, isLast);

        // Get what we know about the parent commit this came from
        GitTreeInfo? parentTree = commit.Parents.Any() ? _trees[commit.Parents.First().Tree.Sha] : null;

        // Detect any Deleted Files
        if (parentTree != null) {
            foreach (string path in parentTree.Files) {
                if (!treeInfo.Contains(path)) {
                    RepositoryFileInfo file = parentTree.Find(path)!;
                    Log.LogDebug($"Detected deleted file {file.Path}");
                    //_options!.FileWriter.WriteFile(file.AsDeletedFile(commit.Sha, commit.Author.When.UtcDateTime));
                }
            }
        }

        // Identify author
        AuthorInfo author = GetOrCreateAuthor(commit.Author, treeInfo.Bytes, true);
        AuthorInfo committer = GetOrCreateAuthor(commit.Author, 0, false);

        // Create the commit summary info.
        CommitInfo info = CreateCommitFromLibGitCommit(commit, author, committer, treeInfo);

        // Write the commit to the appropriate writer
        //_options!.CommitWriter.Write(info);
    }

    private readonly Dictionary<string, string> _pathShas = new();
    private readonly Dictionary<string, RepositoryFileInfo> _fileInfo = new();

    private void WalkTree(Commit commit, Tree tree, GitTreeInfo treeInfo, bool isLast) {
        foreach (TreeEntry treeEntry in tree) {
            if (treeEntry.TargetType == TreeEntryTargetType.Blob) {
                // Ignore the file if it's not an extension we care about
                FileInfo info = new(treeEntry.Path);
                if (true) { //_options!.FileMatchesFilter(info.Extension)) {
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

                    //_options.FileWriter.WriteFile(fileInfo);
                    // At the very end of the analysis, we want to write out the final state of the file
                    if (isLast) {
                        //_options.FileWriter.WriteFile(fileInfo.AsFinalVersion());
                    }
                }
            } else if (treeEntry.TargetType == TreeEntryTargetType.Tree) {
                Tree subTree = (Tree)treeEntry.Target;

                WalkTree(commit, subTree, treeInfo, isLast);
            }
        }
    }

    private FileState DetermineFileState(string fileLower, TreeEntry treeEntry) {
        if (!_pathShas.TryGetValue(fileLower, out string? value)) {
            // If we didn't have the path before, this is an added file so let's mark it as an added file
            return FileState.Added;
        }

        return value == treeEntry.Target.Sha
            ? FileState.Unmodified
            : FileState.Modified;
    }

    private static RepositoryFileInfo BuildFileInfo(Commit commit, TreeEntry treeEntry, RepositoryFileInfo? oldInfo) {
        Blob blob = (Blob)treeEntry.Target;

        int lines = CountLinesInFile(blob);

        int linesDelta = lines - oldInfo?.Lines ?? lines;

        return new RepositoryFileInfo() {
            Name = treeEntry.Name,
            Path = treeEntry.Path,
            Sha = blob.Id.Sha,
            Lines = lines,
            LinesDelta = linesDelta,
            Bytes = (ulong)blob.Size,
            Commit = commit.Id.Sha,
            CreatedDateUtc = commit.Author.When.UtcDateTime,
        };
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
                Id = _authors.Count + 1,
                Email = signature.Email,
                Name = signature.Name,
                IsBot = signature.Name.Contains("[bot]", StringComparison.OrdinalIgnoreCase),
                EarliestCommitDateUtc = signature.When.UtcDateTime,
                LatestCommitDateUtc = signature.When.UtcDateTime,
                NumCommits = isAuthor ? 1 : 0,
                TotalSizeInBytes = bytes
            };
            Observers.ForEach(o => o.OnNewAuthor(author));
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
            Message = commit.MessageShort, // This is just the first line of the commit message. Usually all that's needed
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
        /*
        if (_options != null) {
            _options.AuthorWriter.Dispose();
            _options.CommitWriter.Dispose();
            _options.FileWriter.Dispose();
            _options = null;
        }
        */
    }
}