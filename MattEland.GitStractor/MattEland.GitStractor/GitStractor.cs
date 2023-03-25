namespace MattEland.GitStractor;

public class GitStractor
{
    public void ExtractCommitInformation(string repoPath, string outputPath)
    {
        // Do something
        LibGit2Sharp.Repository repo = new(repoPath);

        repo.Commits.ToList().ForEach(c => Console.WriteLine(c.Message.Trim()));
    }
}