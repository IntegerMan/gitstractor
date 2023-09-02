using GitStractor.Model;

namespace GitStractor.GitObservers;

public class GitAuthorObserver : FileWriterObserverBase
{
    public override string Filename => "Authors.csv";

    public override void OnBeginningIteration(int totalCommits, string outputPath)
    {
        base.OnBeginningIteration(totalCommits, outputPath);

        WriteField("Id");
        WriteField("Email");
        WriteField("Name");
        WriteField("Is Bot?");
        WriteField("First Commit Date UTC");
        NextRecord();
    }

    public override void OnNewAuthor(AuthorInfo author)
    {
        base.OnNewAuthor(author);

        WriteField(author.Id);
        WriteField(author.Email);
        WriteField(author.Name);
        WriteField(author.IsBot);
        WriteField(author.EarliestCommitDateUtc);
        NextRecord();
    }
}