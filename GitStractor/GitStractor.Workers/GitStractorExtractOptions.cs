using System.ComponentModel.DataAnnotations;

namespace GitStractor.Workers;

[Serializable]
public class GitStractorExtractOptions
{
    [Required(ErrorMessage = "Source path is required. Specify via -s or --source")]
    public required string SourcePath { get; init; }

    [Required(ErrorMessage="Output path is required. Specify via -d or --destination")]
    public required string OutputPath { get; init; }

    public bool IncludeBranchDetails { get; init; } = true;


    public string? AuthorMapPath { get; init; }

    public string IgnorePatterns { get; init; } = string.Empty;
}
