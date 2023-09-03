namespace GitStractor.Model;

public class GitTreeInfo
{
    private readonly HashSet<RepositoryFileInfo> _files = new();
    public int TotalFileCount => _files.Count;
    public int AddedFileCount => _files.Count(f => f.State == FileState.Added);
    public int DeletedFileCount => _files.Count(f => f.State == FileState.Deleted);
    public int ChangedFileCount => _files.Count(f => f.State is FileState.Added or FileState.Modified or FileState.Deleted);
    public int LinesAdded { get; private set; }
    public int LinesDeleted { get; private set; }

    public void Register(RepositoryFileInfo file)
    {
        _files.Add(file);

        LinesAdded += file.LinesAdded;
        LinesDeleted += file.LinesDeleted;
    }
}