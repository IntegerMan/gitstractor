using LibGit2Sharp;

public class Program {

    private const int SuccessCode = 0;
    private const int ValidationErrorCode = -1;
    private const int BadGitUrlErrorCode = -2;
    private const int FolderAlreadyExistsErrorCode = -2;
    private const int BadRepoErrorCode = -4;

    public static int Main(string[] args) {
        // Validate arguments
        if (args == null || args.Length != 2) {
            Console.WriteLine($"Invalid usage called with arguments {string.Join(' ', args ?? Array.Empty<string>())}");
            Console.WriteLine();
            Console.WriteLine(@"Expected usage: 'Gitstractor-Acquire ""git_repo_url.git"" ""C:\Path\On\Disk""");

            return ValidationErrorCode;
        }

        // Extract arguments
        string gitRepo = args[0];
        string clonePath = args[1];

        // Attempt to clone the repository
        try {
            Console.WriteLine($"Cloning '{gitRepo}' to {clonePath}");
            string resultingPath = Repository.Clone(gitRepo, clonePath);

            Console.WriteLine($"Repository cloned to '{resultingPath}'");

            return SuccessCode;
        }
        catch (RecurseSubmodulesException ex) {
            Console.WriteLine($"The repository has recursive submodules and cannot be cloned: {ex.Message}");

            return BadRepoErrorCode;
        }
        catch (NameConflictException ex) {
            Console.WriteLine(ex.Message); // this exception's details are pretty helpful so just print it to the screen

            return FolderAlreadyExistsErrorCode;
        }
        catch (LibGit2SharpException ex) {
            if (ex.Message.Contains("401")) {
                Console.WriteLine($"The repository '{gitRepo}' could not be found and may be private. Check the URL and access settings.");

                return BadGitUrlErrorCode;
            } else if (ex.Message.Contains("unsupported", StringComparison.OrdinalIgnoreCase)) {
                Console.WriteLine($"Could not clone repository '{gitRepo}': {ex.Message}");

                return BadGitUrlErrorCode;
            } else if (ex.Message.Contains("unexpected content-type", StringComparison.OrdinalIgnoreCase)) {
                Console.WriteLine($"Could not find a git repository at '{gitRepo}'");

                return BadGitUrlErrorCode;
            } else {
                throw;
            }

        }
    }
}