using CommandLine;
using LibGit2Sharp;

namespace GitStractor.Acquire;

public class Program
{
    private const int SuccessCode = 0;
    private const int ValidationErrorCode = -1;
    private const int BadGitUrlErrorCode = -2;
    private const int FolderAlreadyExistsErrorCode = -2;
    private const int BadRepoErrorCode = -4;
    private const int DiskWriteErrorCode = -5;
    private const int UnknownErrorCode = -6;

    public static int Main(string[] args)
    {
        // Handle parsing and validation
        ParserResult<GitStractorAcquireOptions> parserResult =
            Parser.Default.ParseArguments<GitStractorAcquireOptions>(args);
       
        if (parserResult.Tag == ParserResultType.NotParsed)
        {
            return ValidationErrorCode;
        }

        // Extract arguments
        GitStractorAcquireOptions options = parserResult.Value;
        options.ExtractPath ??= Environment.CurrentDirectory;

        // TODO: Report to app insights

        // Attempt to clone the repository
        try
        {
            // If the user specified they should delete the directory and it exists, delete it
            if (options.OverwriteIfExists && Directory.Exists(options.ExtractPath))
            {
                Console.WriteLine($"Deleting existing directory {options.ExtractPath}");
                Directory.Delete(options.ExtractPath, recursive: true);
            }

            Console.WriteLine($"Cloning '{options.Repository}' to {options.ExtractPath}");
            string resultingPath = Repository.Clone(options.Repository, options.ExtractPath);

            Console.WriteLine($"Repository cloned to '{resultingPath}'");

            // TODO: Report to app insights

            // TODO: Enqueue processing of this repository on RabbitMQ or an Azure Queue

            // TODO: Report to app insights

            return SuccessCode;
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"Failed to delete directory {options.ExtractPath}: {ex.Message}");
            return DiskWriteErrorCode;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine($"Insufficient permissions for directory {options.ExtractPath}: {ex.Message}");
            return DiskWriteErrorCode;
        }
        catch (RecurseSubmodulesException ex)
        {
            ReportException(ex, $"The repository has recursive submodules and cannot be cloned: {ex.Message}");
            return BadRepoErrorCode;
        }
        catch (NameConflictException ex)
        {
            ReportException(ex, ex.Message); // this exception's details are pretty helpful here
            return FolderAlreadyExistsErrorCode;
        }
        catch (LibGit2SharpException ex)
        {
            return HandleLibGit2SharpException(ex, options);
        }
    }

    private static int HandleLibGit2SharpException(LibGit2SharpException ex, GitStractorAcquireOptions options)
    {
        if (ex.Message.Contains("401"))
        {
            ReportException(ex,
                $"The repository '{options.Repository}' could not be found and may be private. Check the URL and access settings.");
            return BadGitUrlErrorCode;
        }

        if (ex.Message.Contains("unsupported", StringComparison.OrdinalIgnoreCase))
        {
            ReportException(ex, $"Could not clone repository '{options.Repository}': {ex.Message}");
            return BadGitUrlErrorCode;
        }

        if (ex.Message.Contains("unexpected content-type", StringComparison.OrdinalIgnoreCase))
        {
            ReportException(ex, $"Could not find a git repository at '{options.Repository}'");
            return BadGitUrlErrorCode;
        }

        if (ex.Message.Contains("failed to make directory", StringComparison.OrdinalIgnoreCase))
        {
            ReportException(ex, $"Could not save the repository to disk: {ex.Message}");
            return DiskWriteErrorCode;
        }

        ReportException(ex, $"An unexpected error occurred cloning '{options.Repository}': {ex.GetType().Name} {ex.Message}");
        return UnknownErrorCode;
    }

    private static void ReportException(Exception ex, string userFacingMessage)
    {
        Console.WriteLine(userFacingMessage);

        // TODO: Report to app insights
    }

}