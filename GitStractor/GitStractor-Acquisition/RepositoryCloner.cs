using LibGit2Sharp;

namespace GitStractor.Acquire;

public class RepositoryCloner
{
    public CloneResult Clone(string repository, string? extractPath, bool overwriteIfExists = false)
    {
        extractPath ??= Environment.CurrentDirectory;

        try
        {
            // If the user specified they should delete the directory and it exists, delete it
            if (overwriteIfExists && Directory.Exists(extractPath))
            {
                Console.WriteLine($"Deleting existing directory {extractPath}");
                Directory.Delete(extractPath, recursive: true);
            }

            Console.WriteLine($"Cloning '{repository}' to {extractPath}");
            string resultingPath = Repository.Clone(repository, extractPath);

            Console.WriteLine($"Repository cloned to '{resultingPath}'");

            // TODO: Report to app insights

            // TODO: Enqueue processing of this repository on RabbitMQ or an Azure Queue

            // TODO: Report to app insights

            return CloneResult.Success;
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine($"Failed to delete directory {extractPath}: {ex.Message}");
            return CloneResult.DiskWriteError;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine($"Insufficient permissions for directory {extractPath}: {ex.Message}");
            return CloneResult.DiskWriteError;
        }
        catch (RecurseSubmodulesException ex)
        {
            ReportException(ex, $"The repository has recursive submodules and cannot be cloned: {ex.Message}");
            return CloneResult.BadRepository;
        }
        catch (NameConflictException ex)
        {
            ReportException(ex, ex.Message); // this exception's details are pretty helpful here
            return CloneResult.FolderAlreadyExists;
        }
        catch (LibGit2SharpException ex)
        {
            return HandleLibGit2SharpException(ex, repository);
        }
    }

    private CloneResult HandleLibGit2SharpException(Exception ex, string repository)
    {
        if (ex.Message.Contains("401"))
        {
            ReportException(ex,
                $"The repository '{repository}' could not be found and may be private. Check the URL and access settings.");
            return CloneResult.BadGitUrl;
        }

        if (ex.Message.Contains("unsupported", StringComparison.OrdinalIgnoreCase))
        {
            ReportException(ex, $"Could not clone repository '{repository}': {ex.Message}");
            return CloneResult.BadGitUrl;
        }

        if (ex.Message.Contains("unexpected content-type", StringComparison.OrdinalIgnoreCase))
        {
            ReportException(ex, $"Could not find a git repository at '{repository}'");
            return CloneResult.BadGitUrl;
        }

        if (ex.Message.Contains("failed to make directory", StringComparison.OrdinalIgnoreCase))
        {
            ReportException(ex, $"Could not save the repository to disk: {ex.Message}");
            return CloneResult.DiskWriteError;
        }

        ReportException(ex, $"An unexpected error occurred cloning '{repository}': {ex.GetType().Name} {ex.Message}");
        return CloneResult.UnknownError;
    }

    private void ReportException(Exception ex, string userFacingMessage)
    {
        Console.WriteLine(userFacingMessage);

        // TODO: Report to app insights
    }
}