using GitStractor.Model;

namespace GitStractor.GitObservers;

public class SummaryAuthorObserver : FileWriterObserverBase
{
    public override string Filename => "Authors.csv";
    private readonly List<AuthorInfo> _authors = new();

    public override void OnBeginningIteration(int totalCommits, string outputPath)
    {
        base.OnBeginningIteration(totalCommits, outputPath);

        WriteField("Id");
        WriteField("Email");
        WriteField("Name");
        WriteField("Is Bot?");
        WriteField("Num Commits");
        WriteField("Lines Added");
        WriteField("Lines Deleted");
        WriteField("First Commit Date UTC");
        WriteField("Last Commit Date UTC");
        NextRecord();
    }

    public override void OnNewAuthor(AuthorInfo author)
    {
        base.OnNewAuthor(author);

        _authors.Add(author);

    }

    public override void OnCompletedIteration(string outputPath) {
        foreach (AuthorInfo author in _authors) {
            WriteField(author.Id);
            WriteField(author.Email);
            WriteField(author.Name);
            WriteField(author.IsBot);
            WriteField(author.NumCommits);
            WriteField(author.LinesAdded);
            WriteField(author.LinesDeleted);
            WriteField(author.EarliestCommitDateUtc);
            WriteField(author.LatestCommitDateUtc);
            NextRecord();
        }

        base.OnCompletedIteration(outputPath);
    }
}