using GitStractor.Model;

namespace GitStractor.GitObservers;

public class StreamingAuthorObserver : FileWriterObserverBase
{
    public override string Filename => "Authors.csv";

    public override void OnBeginningIteration(int totalCommits, string outputPath, bool includeBranchDetails)
    {
        base.OnBeginningIteration(totalCommits, outputPath, includeBranchDetails);

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