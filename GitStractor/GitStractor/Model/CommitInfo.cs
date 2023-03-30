namespace GitStractor.Model;

/// <summary>
/// This class is an abstraction representing another commit.
/// It exists so that changes to LibGit2Sharp do not propagate throughout the application.
/// </summary>
public class CommitInfo
{
    private readonly List<string> _files;

    /// <summary>
    /// Creates a new instance of <see cref="CommitInfo"/>.
    /// </summary>
    /// <param name="files">The files associated with the commit</param>
    public CommitInfo(IEnumerable<string> files)
    {
        _files = new List<string>(files);
    }

    /// <summary>
    /// The commit message
    /// </summary>
    public required string Message { get; init; }
    
    /// <summary>
    /// The total blob size of the commit in terms of bytes
    /// </summary>
    public ulong SizeInBytes { get; init; }
    
    /// <summary>
    /// Gets the total number of files in the working tree as of this commit. This is typically not the amount of files
    /// modified by the commit.
    /// </summary>
    public uint TotalFiles { get; init; }

    /// <summary>
    /// The SHA of the commit
    /// </summary>
    public required string Sha { get; init; }

    /// <summary>
    /// The person who wrote the contents of the commit
    /// </summary>
    public required AuthorInfo Author { get; init; }

    /// <summary>
    /// The date the commit was authored in UTC time
    /// </summary>
    public required DateTime AuthorDateUtc { get; init; }

    /// <summary>
    /// The <see cref="AuthorDateUtc"/> in local time
    /// </summary>
    public DateTime AuthorDateLocal => AuthorDateUtc.ToLocalTime();

    /// <summary>
    /// The person who committed the commit
    /// This is usually the same as <see cref="Author"/> but may represent someone committing the author's changes.
    /// </summary>
    public required AuthorInfo Committer { get; init; }

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

    /// <summary>
    /// The number of files in the commit's tree
    /// </summary>
    public uint NumFiles => (uint)(_files.Count + DeletedFiles);
    
    /// <summary>
    /// The number of files in this commit's tree that didn't appear with the same path previously
    /// </summary>
    public uint AddedFiles { get; set; }
    
    /// <summary>
    /// The number of files in the prior commit's tree that didn't appear in this tree's
    /// </summary>
    public uint DeletedFiles { get; set; }
    
    /// <summary>
    /// The names of the files modified by the commit
    /// </summary>
    public IReadOnlyList<string> Files => _files;

    /// <summary>
    /// A comma-separated list of files modified by the commit
    /// </summary>
    public string FileNames => string.Join(", ", Files);

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Sha[..6]} {Author.Name} @ {AuthorDateLocal.ToShortDateString()} {AuthorDateLocal.ToShortTimeString()}: {Message} ({NumFiles} file(s), +{AddedFiles}/-{DeletedFiles})";
    }
}