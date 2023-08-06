using CommandLine;

namespace GitStractor.Acquire;

public class Program
{
    public static int Main(string[] args)
    {
        // Handle parsing and validation
        ParserResult<GitStractorAcquireOptions> parserResult =
            Parser.Default.ParseArguments<GitStractorAcquireOptions>(args);
       
        if (parserResult.Tag == ParserResultType.NotParsed)
        {
            return (int) CloneResult.ValidationError;
        }

        // Extract arguments
        GitStractorAcquireOptions options = parserResult.Value;

        // TODO: Report to app insights

        // Attempt to clone the repository
        RepositoryCloner cloner = new();
        CloneResult result = cloner.Clone(options.Repository, options.ExtractPath, options.OverwriteIfExists);

        return (int) result;
    }
}