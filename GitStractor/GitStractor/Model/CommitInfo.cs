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
    /// The name of the person who wrote the contents of the commit
    /// </summary>
    public required string AuthorName { get; init; }
    
    /// <summary>
    /// The email address of the person who wrote the contents of the commit
    /// </summary>
    public required string AuthorEmail { get; init; }

    /// <summary>
    /// The date the commit was authored in UTC time
    /// </summary>
    public required DateTime AuthorDateUtc { get; init; }

    /// <summary>
    /// The <see cref="AuthorDateUtc"/> in local time
    /// </summary>
    public DateTime AuthorDateLocal => AuthorDateUtc.ToLocalTime();


    /// <summary>
    /// The name of the person who committed the code.
    /// This is usually the same as <see cref="AuthorName"/> but may represent someone committing the author's changes.
    /// </summary>
    public required string CommitterName { get; init; }

    /// <summary>
    /// The email address of the person who committed the commit
    /// This is usually the same as <see cref="AuthorEmail"/> but may represent someone committing the author's changes.
    /// </summary>
    public required string CommitterEmail { get; init; }

    /// <summary>
    /// The date the commit was committed in UTC time
    /// This is usually the same as <see cref="AuthorDateUtc"/> but may represent someone committing the author's changes.
    /// </summary>
    public required DateTime CommitterDateUtc { get; init; }

    /// <summary>
    /// The <see cref="CommitterDateUtc"/> in local time
    /// This is usually the same as <see cref="AuthorDateLocal"/> but may represent someone committing the author's changes.
    /// </summary>
    public DateTime CommitterDateLocal => CommitterDateUtc.ToLocalTime();

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Sha[..5]} {AuthorName} @ {AuthorDateLocal.ToShortDateString()} {AuthorDateLocal.ToShortTimeString()}: {Message}";
    }
}