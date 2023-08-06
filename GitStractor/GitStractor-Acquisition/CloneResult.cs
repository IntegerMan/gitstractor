namespace GitStractor.Acquire;

/// <summary>
/// This enum mostly exists for generating status codes from the command line
/// </summary>
public enum CloneResult
{
    Success = 0,
    ValidationError = -1,
    BadGitUrl = -2,
    FolderAlreadyExists = -3,
    BadRepository = -4,
    DiskWriteError = -5,
    UnknownError = -6
}