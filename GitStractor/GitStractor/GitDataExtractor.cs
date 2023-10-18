using GitStractor.GitObservers;
using GitStractor.Model;
using GitStractor.Utilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

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
    public void ExtractInformation(string repoPath, string outputPath, string? authorMapPath, bool includeBranchDetails, string[] ignorePatterns) {

        // Clear old state
        _authors.Clear();

        // If we got a git directory that isn't actually a git directory, look for a .git file in its parents
        string? gitPath = FileUtilities.GetParentGitDirectory(repoPath);

        // If we didn't find a git directory, throw an exception
        if (gitPath == null) {
            string message = $"Could not find a git repository at {repoPath}";
            Log.LogWarning(message);
            throw new RepositoryNotFoundException(message);
        }

        // If we got an author map, let's instantiate it
        List<AuthorMap> authorMaps;
        if (!string.IsNullOrEmpty(authorMapPath)) {
            if (!File.Exists(authorMapPath)) { 
                throw new FileNotFoundException("Author map file does not exist", authorMapPath);
            }

            // Deserialize an AuthorMap array from the JSON in the file in the filesystem at authorMapPath
            string json = File.ReadAllText(authorMapPath);

            // Deserialize the JSON into a list of AuthorMap
            authorMaps = JsonConvert.DeserializeObject<List<AuthorMap>>(json)!;

            Log.LogInformation("Using author map from {AuthorMapPath} with {Count} author entries", authorMapPath, authorMaps.Count);
        } else {
            authorMaps = new List<AuthorMap>();
        }

        Log.LogInformation("Analyzing git repository at {Path}", gitPath);

        // Connect to the git repository
        RepositoryOptions repoOptions = new() {
            Identity = new Identity("Test", "Testerson@gmail.com"), // TODO: This could come from the options
        };

        try {
            using Repository repo = new(gitPath, repoOptions);

            // Customize how we'll traverse commit history
            CommitFilter filter = new() {
                // It's very important to walk through commits in historical order from oldest to newest
                // This lets us accurately get a read on the state of the repository at the time of each commit
                SortBy = CommitSortStrategies.Reverse,

                // Don't include branch details
                FirstParentOnly = !includeBranchDetails,

                // We only want to look at commits that are reachable from whatever is HEAD at the moment
                IncludeReachableFrom = repo.Head
            };

            ICommitLog commits = repo.Commits.QueryBy(filter);
            int totalCommits = commits.Count();

            Observers.ForEach(o => o.OnBeginningIteration(totalCommits, outputPath, includeBranchDetails));

            // Loop over each commit
            int commitNum = 0;
            foreach (Commit commit in commits) {
                commitNum++;
                ProcessCommit(commit, repo, authorMaps, ignorePatterns);

                UpdateProgress(totalCommits, commitNum);
            }
            Log.LogInformation("Enumerated {Commits} commits", commitNum);

            // Write all authors at the end, now that we know aggregate-level information
            Observers.ForEach(o => o.OnCompletedIteration(outputPath));

            //_options.ProgressListener?.Completed();
            Log.LogInformation("Successfully analyzed repository at {Path}", repoPath);
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

    private void ProcessCommit(Commit commit, Repository repo, IEnumerable<AuthorMap> authorMap, IEnumerable<string> ignorePatterns) {
        bool isLast = commit == repo.Head.Tip;
        Observers.ForEach(o => o.OnProcessingCommit(commit.Sha, isLast));

        // Identify author
        AuthorInfo author = GetOrCreateAuthor(commit.Author, true, authorMap);
        AuthorInfo committer = GetOrCreateAuthor(commit.Committer, false, authorMap);

        // Create the commit summary info.
        CommitInfo info = CreateCommitFromLibGitCommit(commit, author, committer);

        // Parse Commit
        _treeWalker.WalkCommitTree(commit, info, repo, Observers, ignorePatterns);

        author.LinesDeleted += info.LinesDeleted;
        author.LinesAdded += info.LinesAdded;
        author.FilesAdded += info.FilesAdded;
        author.FilesDeleted += info.FilesDeleted;
        author.FilesModified += info.FilesModified;

        // Write the commit to the appropriate writers
        Observers.ForEach(o => o.OnProcessedCommit(info));
    }

    private AuthorInfo GetOrCreateAuthor(Signature signature, bool isAuthor, IEnumerable<AuthorMap> authorMap) {

        string name = signature.Name;
        string email = signature.Email.ToLowerInvariant();
        bool isBot = signature.Name.Contains("[bot]", StringComparison.OrdinalIgnoreCase);

        AuthorMap? matched = authorMap.FirstOrDefault(m => m.Emails.Any(e => e.Equals(email, StringComparison.OrdinalIgnoreCase)));

        if (matched != null) {
            email = matched.Emails[0];
            name = matched.Name;
            isBot = matched.Bot;
        }

        if (!_authors.TryGetValue(email, out AuthorInfo? author)) {
            author = new AuthorInfo() {
                Id = _authors.Count + 1,
                Email = email,
                Name = name,
                IsBot = isBot,
                EarliestCommitDateUtc = signature.When.UtcDateTime,
                LatestCommitDateUtc = signature.When.UtcDateTime,
                NumCommits = isAuthor ? 1 : 0,
            };
            Observers.ForEach(o => o.OnNewAuthor(author));
            _authors.Add(email, author);
        } else {
            if (isAuthor) {
                author.NumCommits++;
            }

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
        AuthorInfo committer)
        => new() {
            Sha = commit.Sha,
            ParentSha = commit.Parents.FirstOrDefault()?.Sha,
            Parent2Sha = commit.Parents.Count() > 1 ? commit.Parents.Last().Sha : null,
            Message = commit.MessageShort, // This is just the first line of the commit message. Usually all that's needed

            // Author information. Author is the person who wrote the contents of the commit
            Author = author,
            AuthorDateUtc = commit.Author.When.UtcDateTime,

            // Committer information. Committer is the person who performed the commit
            Committer = committer,
            CommitterDateUtc = commit.Committer.When.UtcDateTime,
        };
}