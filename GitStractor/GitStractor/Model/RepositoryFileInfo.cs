namespace GitStractor.Model;

public class RepositoryFileInfo
{
    public FileState State { get; set; } = FileState.Unmodified;
    public required string Sha { get; init; }
    public required string Path { get; set; }

    public string Path1 
        => Path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? string.Empty;

    public string Path2 {
        get {
            string[] parts = Path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts.Length switch {
                > 1 => parts[1],
                _ => string.Empty
            };
        }
    }

    public string Path3 {
        get {
            string[] parts = Path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts.Length switch {
                > 2 => parts[2],
                _ => string.Empty
            };
        }
    }

    public string Extension
    {
        get
        {
            FileInfo info = new(Path);
            return info.Extension;
        }
    }

    public required string Commit { get; init; }
    public DateTime CreatedDateUtc { get; init; }
    public required int Lines { get; init; }
    public string? OldPath { get; init; }
    public int LinesDeleted { get; init; }
    public int LinesAdded { get; init; }

    public override string ToString() 
        => $"{Sha[..6]} {Path} @ {Commit[..6]} ({State})";
}