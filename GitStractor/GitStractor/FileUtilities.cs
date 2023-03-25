namespace GitStractor;

/// <summary>
/// This class offers utility methods for working with git directories
/// </summary>
public static class FileUtilities
{
    /// <summary>
    /// Starts in the current directory and walks upwards towards the drive root
    /// until it finds a .git directory. Once one is found, it returns the path.
    /// </summary>
    /// <param name="directory">
    /// The directory to analyze. This should be a git repository or part of one.
    /// If this is not specified, <c>Environment.CurrentDirectory</c> will be used.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the directory is not part of a git repository
    /// </exception>
    /// <returns>The git folder containing the active repository</returns>
    public static string GetParentGitDirectory(string? directory = null)
    {
        directory ??= Environment.CurrentDirectory;
        
        DirectoryInfo? dirInfo = new(directory);
        
        // Walk up the drive path until we find a git repo
        while (dirInfo != null && !dirInfo.EnumerateDirectories(".git").Any())
        {
            dirInfo = dirInfo.Parent;
        }

        // Handle the case where there was no git repo
        if (dirInfo == null)
        {
            throw new InvalidOperationException("Could not find a .git directory in the current directory or any parent directory.");
        }

        return dirInfo.FullName;
    }

}