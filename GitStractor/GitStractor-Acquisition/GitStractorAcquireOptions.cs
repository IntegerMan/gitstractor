using CommandLine;

namespace GitStractor.Acquire;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class GitStractorAcquireOptions
{
    [Option('r', "repository", Required = true, HelpText = "The path to the repository to clone. This should be a URL to a git repository.")]
    public string Repository { get; set; }

    [Option('p', "path", Required = true, HelpText = "The path to the output folder where the repository will be stored. The .git folder and all repo contents will be added directly to this folder.")]
    public string ExtractPath { get; set; }

    [Option('o', "overwrite", Required = false, Default = false, HelpText = "If true, the output folder will be deleted if it exists before the repository is cloned. Otherwise, the clone will fail if the path exists.")]
    public bool OverwriteIfExists { get; set; }

    public bool IsValid => !string.IsNullOrWhiteSpace(Repository) && !string.IsNullOrWhiteSpace(ExtractPath);
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.