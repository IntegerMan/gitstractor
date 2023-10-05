using System.ComponentModel.DataAnnotations;

namespace GitStractor.Workers;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class GitStractorExtractOptions
{
    [Required(ErrorMessage = "Source path is required. Specify via -s or --source")]
    public string SourcePath { get; set; }

    [Required(ErrorMessage="Output path is required. Specify via -d or --destination")]
    public string OutputPath { get; set; }

    public bool OverwriteIfExists { get; set; }

    public string AuthorMapPath { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.