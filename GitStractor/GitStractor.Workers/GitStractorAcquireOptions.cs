using System.ComponentModel.DataAnnotations;

namespace GitStractor.Workers;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class GitStractorAcquireOptions
{
    [Required(ErrorMessage = "Repository is required. Specify a git repository via -r or --repository")]
    public string Repository { get; set; }

    [Required(ErrorMessage="Extraction folder path is required. Specify extract path via -p or --path")]
    public string ExtractPath { get; set; }

    public bool OverwriteIfExists { get; set; }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.