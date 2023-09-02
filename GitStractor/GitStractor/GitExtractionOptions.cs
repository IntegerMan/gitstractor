using GitStractor.Model;
using GitStractor.Writers;

namespace GitStractor;

/// <summary>
/// This class represents all options that GitStractor needs
/// to analyze a git repository.
/// </summary>
public class GitExtractionOptions
{
    /// <summary>
    /// The path to the repository to analyze.
    /// This should be a local path to a folder on disk.
    /// </summary>
    public required string RepositoryPath { get; init; }

    public IProgressListener? ProgressListener { get; set; }
    
    /// <summary>
    /// A comma-separated list of lowercased file extensions to include in the analysis. This is typically most code files.
    /// </summary>
    public HashSet<string> CodeFileExtensions { get; set; } = new()
    {
        ".cs", ".csx", ".py", ".fs", ".fsx", ".js", ".ts", ".vb", ".vbx", ".java", ".html", ".css", ".md", 
        ".scss", ".less", ".less",".cshtml",".vbhtml",".csx",".fsi",".fsscript",".fsscript", ".fsproj", ".csproj", 
        ".vbproj", ".shproj", ".sqlproj",".xproj", ".njsproj",".nuproj",".sln", ".unityproj", ".vcxproj", ".r", ".agc",
        ".c", ".cpp", ".go", ".asp", ".aspx", ".ascx", ".asmx", ".ashx", ".axd", ".cshtml", ".vbhtml", ".csx", ".jsp",
        ".jspx", ".jhtm", ".jhtml", ".wss", ".do", ".action", ".jsf", ".faces", ".xhtml", ".xht", ".xaml", ".cshtml",
        ".lua", ".php", ".php3", ".php4", ".php5", ".phtml", ".py", ".pyw", ".rhtml", ".rpy", ".rb", ".erb", ".rjs", ".rxml",
    };

    /// <summary>
    /// The <see cref="DataWriterBase"/> used to save author information when processessing commits.
    /// </summary>
    public required AuthorDataWriter AuthorWriter { get; init; }

    /// <summary>
    /// The <see cref="DataWriterBase"/> used to save commit information when processessing commits.
    /// </summary>
    public required CommitDataWriter CommitWriter { get; init; }

    /// <summary>
    /// The <see cref="DataWriterBase"/> used to save information on individual files when processessing commits.
    /// </summary>
    public required FileDataWriter FileWriter { get; init; }

    public static GitExtractionOptions BuildConsoleOptions(string repositoryPath)
        => new()
        {
            RepositoryPath = repositoryPath,
            AuthorWriter = new AuthorConsoleDataWriter(),
            CommitWriter = new CommitConsoleDataWriter(),
            FileWriter = new FileConsoleDataWriter()
        };

    public static GitExtractionOptions BuildInMemoryOptions(string repositoryPath)
        => new()
        {
            RepositoryPath = repositoryPath,
            AuthorWriter = new AuthorInMemoryDataWriter(),
            CommitWriter = new CommitInMemoryDataWriter(),
            FileWriter = new FileInMemoryDataWriter()
        };

    public static GitExtractionOptions BuildFileOptions(string repositoryPath, string? outputDirectory = null)
    {
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            outputDirectory = Environment.CurrentDirectory;
        }

        return new GitExtractionOptions
        {
            RepositoryPath = repositoryPath,
            AuthorWriter = new AuthorCsvDataWriter(Path.Combine(outputDirectory, "Authors.csv")),
            CommitWriter = new CommitCompoundDataWriter(new[]
            {
                new CommitCsvDataWriter(Path.Combine(outputDirectory, "Commits.csv")),
                new FileCommitCsvDataWriter(Path.Combine(outputDirectory, "FileCommits.csv")),
            }),
            FileWriter = new FileCompoundDataWriter(Array.Empty<FileDataWriter>())
        };
    }

    public bool FileMatchesFilter(string extension) => CodeFileExtensions.Contains(extension.ToLowerInvariant());
}