namespace GitStractor.Model;

public class GitTreeInfo
{
    private readonly HashSet<RepositoryFileInfo> _files = new();
    public int TotalFileCount => _files.Count;
    public int AddedFileCount => _files.Count(f => f.State == FileState.Added);
    public int DeletedFileCount => _files.Count(f => f.State == FileState.Deleted);
    public int ChangedFileCount => _files.Count(f => f.State is FileState.Added or FileState.Modified or FileState.Deleted);

    public IEnumerable<string> Files => _files.Select(f => f.Path);
    public IEnumerable<RepositoryFileInfo> ModifiedFiles => _files.Where(f => f.State != FileState.Unmodified);

    public int LinesAdded { get; private set; }
    public int LinesDeleted { get; private set; }

    public bool Contains(string path) => _files.Select(f => f.Path).Contains(path);

    public void Register(RepositoryFileInfo file)
    {
        if (Contains(file.Path))
            return;

        _files.Add(file);

        LinesAdded += file.LinesAdded;
        LinesDeleted += file.LinesDeleted;
    }

    public RepositoryFileInfo? Find(string path)
        => _files.FirstOrDefault(f => string.Equals(f.Path, path, StringComparison.InvariantCultureIgnoreCase));
}