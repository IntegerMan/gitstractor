using CommandLine;

namespace GitStractor.Acquire;

public class GitStractorAcquireOptions
{
    [Option('r', "repository", Required = true, HelpText = "The path to the repository to clone. This should be a URL to a git repository.")]
    public required string Repository { get; init; }

    [Option('p', "path", Required = false, Default = null, HelpText = "The path to the output folder where the repository will be stored. The .git folder and all repo contents will be added directly to this folder. Defaults to the current directory.")]
    public string? ExtractPath { get; set; }

    [Option('o', "overwrite", Required = false, Default = false, HelpText = "If true, the output folder will be deleted if it exists before the repository is cloned. Otherwise, the clone will fail if the path exists.")]
    public bool OverwriteIfExists { get; set; }
}