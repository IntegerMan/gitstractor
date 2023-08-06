using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace GitStractor.Acquire;

public class RepositoryCloner {
    public ILogger<RepositoryCloner> Log { get; }

    public RepositoryCloner(ILogger<RepositoryCloner> log) {
        Log = log;
    }

    /// <summary>
    /// Clones a repository to disk at the specified path
    /// </summary>
    /// <param name="repository">The path or URL to clone the repository from. This should be a public repository</param>
    /// <param name="extractPath">The path on disk to clone the repository to</param>
    /// <param name="overwriteIfExists">If this is true, existing contents of the directory will be removed when cloning into a non-empty directory. Defaults to false.</param>
    /// <returns>The path of the git directory of the repository that was created</returns>
    /// <exception cref="CloneException">Thrown if there was a difficulty resolving the repository or cloning it to disk</exception>
    public string Clone(string repository, string extractPath, bool overwriteIfExists = false) {
        ArgumentException.ThrowIfNullOrEmpty(repository, nameof(repository));
        ArgumentException.ThrowIfNullOrEmpty(extractPath, nameof(extractPath));

        try {
            // If the user specified a directory and it exists, we need to react to that
            HandleExistingDirectory(repository, extractPath, overwriteIfExists);

            Log?.LogInformation($"Cloning '{repository}' to {extractPath}");

            return Repository.Clone(repository, extractPath);
        }
        // There are a lot of things that can go wrong cloning things, but I want to make sure callers only need to worry about one.
        // CloneException should encapsulate anything that can go awry
        catch (IOException ex) {
            throw new CloneException($"Failed to delete directory {extractPath}: {ex.Message}", repository, extractPath, ex);
        }
        catch (UnauthorizedAccessException ex) {
            throw new CloneException($"Insufficient permissions for directory {extractPath}: {ex.Message}", repository, extractPath, ex);
        }
        catch (RecurseSubmodulesException ex) {
            throw new CloneException($"The repository has recursive submodules and cannot be cloned: {ex.Message}", repository, extractPath, ex);
        }
        catch (NameConflictException ex) {
            throw new CloneException(ex.Message, repository, extractPath, ex);
        }
        catch (LibGit2SharpException ex) {
            // LibGit2Sharp wraps a lot of similar exceptions into this catch-all exception, so let's parse the result and wrap it into a better message

            if (ex.Message.Contains("401")) {
                throw new CloneException($"The repository '{repository}' could not be found and may be private. Check the URL and access settings.", repository, extractPath, ex);
            }

            if (ex.Message.Contains("unsupported", StringComparison.OrdinalIgnoreCase)) {
                throw new CloneException($"Could not clone repository '{repository}': {ex.Message}", repository, extractPath, ex);
            }

            if (ex.Message.Contains("unexpected content-type", StringComparison.OrdinalIgnoreCase)) {
                throw new CloneException($"Could not find a git repository at '{repository}'", repository, extractPath, ex);
            }

            if (ex.Message.Contains("failed to make directory", StringComparison.OrdinalIgnoreCase)) {
                throw new CloneException($"Could not save the repository to disk: {ex.Message}", repository, extractPath, ex);
            }

            throw new CloneException($"An unexpected error occurred cloning '{repository}': {ex.GetType().Name} {ex.Message}", repository, extractPath, ex);
        }
    }

    private void HandleExistingDirectory(string repository, string extractPath, bool overwriteIfExists) {
        // We don't really care if the directory doesn't exist
        if (!Directory.Exists(extractPath)) {
            return;
        }

        Log.LogWarning($"Directory already exists: {extractPath}");

        // Optionally allow the user to delete the directory contents before cloning
        if (overwriteIfExists) {
            Log?.LogInformation($"Deleting directory {extractPath}");

            DirectoryInfo directoryInfo = new(extractPath);
            DeleteDirectoryAndFiles(directoryInfo);
        } else {
            throw new CloneException($"Cannot clone to {extractPath} since the directory already exists", repository, extractPath);
        }
    }

    private static void DeleteDirectoryAndFiles(DirectoryInfo dir) {
        foreach (FileInfo file in dir.EnumerateFiles()) {
            // Some .git files are readonly which Delete doesn't like, so let's set the file to non-readonly before deleting
            if (file.IsReadOnly) {
                File.SetAttributes(file.FullName, FileAttributes.Normal);
            }

            // Delete the file
            file.Delete();
        }

        // Go recursive on any subdirectories
        foreach (DirectoryInfo subDir in dir.EnumerateDirectories()) {
            DeleteDirectoryAndFiles(subDir);
        }

        // Delete this directory now that its contents are empty
        dir.Delete();
    }
}