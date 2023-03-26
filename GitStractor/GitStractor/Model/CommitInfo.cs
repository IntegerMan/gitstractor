namespace GitStractor.Model;

/// <summary>
/// This class is an abstraction representing another commit.
/// It exists so that changes to LibGit2Sharp do not propagate throughout the application.
/// </summary>
public class CommitInfo
{
    /// <summary>
    /// The commit message
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// The SHA of the commit
    /// </summary>
    public required string Sha { get; init; }

    /// <summary>
    /// The name of the person who authored the commit
    /// </summary>
    public required string AuthorName { get; init; }
    
    /// <summary>
    /// The email address of the person who authored the commit
    /// </summary>
    public required string AuthorEmail { get; init; }

    /// <summary>
    /// The date the commit was authored in UTC time
    /// </summary>
    public required DateTime AuthorDate { get; init; }

    /// <summary>
    /// The <see cref="AuthorDate"/> in local time
    /// </summary>
    public DateTime LocalAuthorDate => AuthorDate.ToLocalTime();

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Sha[..5]} {AuthorName} @ {LocalAuthorDate.ToShortDateString()} {LocalAuthorDate.ToShortTimeString()}: {Message}";
    }
}