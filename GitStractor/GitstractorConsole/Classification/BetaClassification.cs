using GitStractor.Readers;
using Spectre.Console;

namespace GitstractorConsole.Classification;

public class CommitClassification
{
    public int Run()
    {
        string directory = "/home/matteland/data/gitstractor";
        
        string commitFile = Path.Combine(directory, "Commits.csv");


        string filePath = Path.Combine(directory, commitFile);
        
        AnsiConsole.MarkupLineInterpolated($"Reading commits from [bold yellow]{filePath}[/]");
        
        var commits = CommitsCsvReader.ReadCommits(filePath);
        
        AnsiConsole.MarkupLineInterpolated($"Read [bold yellow]{commits.Count()}[/] commits");

        return 0;
    }
}