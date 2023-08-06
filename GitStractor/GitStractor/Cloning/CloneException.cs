namespace GitStractor.Cloning;

public class CloneException : Exception {
    public CloneException(string message, string repository, string? extractPath, Exception innerException = null) : base(message, innerException) {
        Repository = repository;
        ExtractPath = extractPath;
    }

    public string Repository { get; }
    public string? ExtractPath { get; }
}
