using GitStractor.Model;

namespace GitStractor;

internal class GitTreeInfo
{
    private readonly HashSet<RepositoryFileInfo> _files = new();
    public ulong Bytes { get; private set; }
    public int TotalFileCount => _files.Count();
    public int AddedFileCount => _files.Count(f => f.State == FileState.Added);
    public int DeletedFileCount => _files.Count(f => f.State == FileState.Deleted);
    public int ChangedFileCount => _files.Count(f => f.State is FileState.Added or FileState.Modified or FileState.Deleted);
    
    public IEnumerable<string> Files => _files.Select(f => f.Path);
    public IEnumerable<string> ModifiedFiles => _files.Where(f => f.State != FileState.Unmodified).Select(f => f.Path);

    public bool Contains(string path) => _files.Select(f => f.Path).Contains(path);
    
    public void Register(RepositoryFileInfo file)
    {
        if (Contains(file.Path))
            return;
        
        _files.Add(file);

        if (file.State != FileState.Unmodified)
        {
            Bytes += file.Bytes;
        }
    }

    public RepositoryFileInfo? Find(string path) 
        => _files.FirstOrDefault(f => string.Equals(f.Path, path, StringComparison.InvariantCultureIgnoreCase));
}