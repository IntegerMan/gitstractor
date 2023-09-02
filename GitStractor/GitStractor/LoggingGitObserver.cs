using GitStractor.Model;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace GitStractor;

public class LoggingGitObserver : IGitObserver {

    public LoggingGitObserver(ILogger<LoggingGitObserver> logger) {
        Log = logger;
    }

    public ILogger<LoggingGitObserver> Log { get; }

    public void OnBeginningIteration(int totalCommits, string outputPath) {
        Log.LogDebug($"Analyzing {totalCommits} commits and writing to files in {outputPath}");
    }

    public void OnCompletedIteration(string outputPath) {
        Log.LogInformation($"Finished analyzing git repository. Output files will be in {outputPath}");
    }

    public void OnNewAuthor(AuthorInfo author) {
        Log.LogInformation($"Found new author: {author.Name} ({author.Email})");
    }

    public void OnProcessingCommit(string sha, bool isLast) {
        Log.LogDebug($"Analyzing commit {sha.Substring(0, 5)}");
    }

    public void OnProcessedCommit(CommitInfo commit) {
        string displayMessage = commit.Message.Substring(0, Math.Min(commit.Message.Length, 140));
        Log.LogDebug($"Analyzed commit {commit.Sha.Substring(0, 5)} by {commit.Author.Name}: {displayMessage}");
    }

    public void OnProcessingFile(RepositoryFileInfo fileInfo, string commitSha) {
    }

    public void UpdateProgress(double percent, int commitNum, double totalCommits) {
        string progressMessage = $"Progress: {percent:p2} ({commitNum}/{totalCommits})";
        Log.LogDebug(progressMessage);
    }
}
