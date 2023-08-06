using CommandLine;
using System.ComponentModel.DataAnnotations;

namespace GitStractor.Workers;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class GitStractorAcquireOptions
{
    [Required]
    public string Repository { get; set; }

    [Required]
    public string ExtractPath { get; set; }

    public bool OverwriteIfExists { get; set; }

    public bool IsValid => !string.IsNullOrWhiteSpace(Repository) && !string.IsNullOrWhiteSpace(ExtractPath);
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.