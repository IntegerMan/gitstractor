using GitStractor.GitObservers;
using GitStractor.Model;
using Spectre.Console;

namespace GitstractorConsole;

public class AnsiConsoleProgressLogger : IGitObserver
{
    private ProgressTask? _task;
    private readonly ProgressContext _context;

    public AnsiConsoleProgressLogger(ProgressContext context)
    {
        _context = context;
    }

    public void OnBeginningIteration(int totalCommits, string outputPath, bool includeBranchDetails)
    {
        _task = _context.AddTask("Extracting git data...", autoStart: false);
        _task.Description($"Analyzing {totalCommits} commits...");
        _task.Value = 0;
        _task.MaxValue = totalCommits;
        _task.StartTask();
    }

    public void OnCompletedIteration(string outputPath)
    {
        _task?.StopTask();
        _task = null;
    }

    public void OnNewAuthor(AuthorInfo author)
    {
    }

    public void OnProcessingCommit(string sha, bool isLast)
    {
    }

    public void OnProcessedCommit(CommitInfo commit)
    {
    }

    public void OnProcessingFile(RepositoryFileInfo fileInfo, CommitInfo commitInfo)
    {
    }

    public void UpdateProgress(double percent, int commitNum, double totalCommits)
    {
        if (_task != null)
        {
            _task.Value = commitNum;
            _task.MaxValue = totalCommits;
        }
    }

}