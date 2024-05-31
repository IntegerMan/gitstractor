using GitStractor.GitObservers;
using GitStractor.Model;
using Spectre.Console;

namespace GitstractorConsole.Extraction;

public class AnsiConsoleCommitLogger : IGitObserver
{
    public void OnBeginningIteration(int totalCommits, string outputPath, bool includeBranchDetails)
    {
    }

    public void OnCompletedIteration(string outputPath)
    {
    }

    public void OnNewAuthor(AuthorInfo author)
    {
    }

    public void OnProcessingCommit(string sha, bool isLast)
    {
    }

    public void OnProcessedCommit(CommitInfo commit)
    {
        AnsiConsole.MarkupLine($"[bold cyan]{commit.AuthorDateLocal.ToShortDateString(),12}[/] [bold blue]{commit.AuthorDateLocal.ToShortTimeString(),10}[/] - [bold yellow]{commit.Author.Name}[/]: {commit.Message}");
    }

    public void OnProcessingFile(RepositoryFileInfo fileInfo, CommitInfo commitInfo)
    {
    }

    public void UpdateProgress(double percent, int commitNum, double totalCommits)
    {
    }
}