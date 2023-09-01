using GitStractor.Model;
using GitStractor.Utilities;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using System;

namespace GitStractor;

/// <summary>
/// This class is the main entry point for using GitDataExtractor to extract information from a Git repository.
/// </summary>
public class GitDataExtractor {
    private const string SecurityMessage = "GitStractor requires the directory to be marked as safe. You can mark all directories as safe globally via the following git command: git config --global --add safe.directory \"*\"";
    private readonly Dictionary<string, AuthorInfo> _authors = new();
    private readonly GitTreeWalker _treeWalker;

    private ILogger<GitDataExtractor> Log { get; }
    public List<IGitObserver> Observers { get; }

    public GitDataExtractor(ILogger<GitDataExtractor> log, IEnumerable<IGitObserver> observers, GitTreeWalker treeWalker) {
        Log = log;
        Observers = observers.ToList();
        _treeWalker = treeWalker;
    }

    /// <exception cref="RepositoryNotFoundException">
    /// Thrown when the repository does not exist
    /// </exception>
    public void ExtractInformation(string repoPath, string outputPath) {

        // Clear old state
        _authors.Clear();
        _treeWalker.Clear();

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

                // Don't include branch details
                FirstParentOnly = true,

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

        GitTreeInfo treeInfo = _treeWalker.WalkCommitTree(commit);

        // Identify author
        AuthorInfo author = GetOrCreateAuthor(commit.Author, true);
        AuthorInfo committer = GetOrCreateAuthor(commit.Author, false);

        // Create the commit summary info.
        CommitInfo info = CreateCommitFromLibGitCommit(commit, author, committer, treeInfo);

        // Write the commit to the appropriate writers
        Observers.ForEach(o => o.OnProcessingCommit(info, isLast));
    }

    private AuthorInfo GetOrCreateAuthor(Signature signature, bool isAuthor) {
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
            };
            Observers.ForEach(o => o.OnNewAuthor(author));
            _authors.Add(key, author);
        } else {
            author.NumCommits += isAuthor ? 1 : 0;

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
            ParentSha = commit.Parents.FirstOrDefault()?.Sha,
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
}