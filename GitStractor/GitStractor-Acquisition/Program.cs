using LibGit2Sharp;

public class Program {

    private const int SuccessCode = 0;
    private const int ValidationErrorCode = -1;
    private const int BadGitUrlErrorCode = -2;
    private const int FolderAlreadyExistsErrorCode = -2;
    private const int BadRepoErrorCode = -4;
    private const int FolderIOErrorCode = -5;

    public static int Main(string[] args) {
        // Validate arguments
        // TODO: There are C# libraries that can work with this in a very nice and object-oriented way
        if (args == null || args.Length < 1 || args.Length > 2) {
            Console.WriteLine($"Invalid usage called with arguments {string.Join(' ', args ?? Array.Empty<string>())}");
            Console.WriteLine();
            Console.WriteLine(@"Expected usage: 'Gitstractor-Acquire ""git_repo_url.git"" ""C:\Path\On\Disk""");

            // TODO: Report to app insights

            return ValidationErrorCode;
        }

        // Extract arguments
        string gitRepo = args[0];
        string clonePath = args.Length == 2 ? args[1] : Environment.CurrentDirectory;

        // TODO: Report to app insights

        // Attempt to clone the repository
        try {
            Console.WriteLine($"Cloning '{gitRepo}' to {clonePath}");
            string resultingPath = Repository.Clone(gitRepo, clonePath);

            Console.WriteLine($"Repository cloned to '{resultingPath}'");

            // TODO: Report to app insights

            // TODO: Enqueue processing of this repository on RabbitMQ or an Azure Queue

            // TODO: Report to app insights

            return SuccessCode;
        }
        catch (RecurseSubmodulesException ex) {
            ReportException(ex, $"The repository has recursive submodules and cannot be cloned: {ex.Message}");
            return BadRepoErrorCode;
        }
        catch (NameConflictException ex) {
            ReportException(ex, ex.Message); // this exception's details are pretty helpful here
            return FolderAlreadyExistsErrorCode;
        }
        catch (LibGit2SharpException ex) {
            if (ex.Message.Contains("401")) {
                ReportException(ex, $"The repository '{gitRepo}' could not be found and may be private. Check the URL and access settings.");
                return BadGitUrlErrorCode;
            } else if (ex.Message.Contains("unsupported", StringComparison.OrdinalIgnoreCase)) {
                ReportException(ex, $"Could not clone repository '{gitRepo}': {ex.Message}");
                return BadGitUrlErrorCode;
            } else if (ex.Message.Contains("unexpected content-type", StringComparison.OrdinalIgnoreCase)) {
                ReportException(ex, $"Could not find a git repository at '{gitRepo}'");
                return BadGitUrlErrorCode;
            } else if (ex.Message.Contains("failed to make directory ", StringComparison.OrdinalIgnoreCase)) {
                ReportException(ex, $"Could not save the repository to disk: {ex.Message}");
                return FolderIOErrorCode;
            } else {
                ReportException(ex, $"An unexpected error occurred cloning '{gitRepo}': {ex.GetType().Name} {ex.Message}");

                // This is something that didn't come up in testing. Let's throw it and let the invoking process know about it
                throw;
            }

        }

    }

    private static void ReportException(Exception ex, string userFacingMessage) {
        Console.WriteLine(userFacingMessage);

        // TODO: Report to app insights
    }

}