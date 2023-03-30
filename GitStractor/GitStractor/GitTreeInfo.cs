﻿namespace GitStractor;

internal class GitTreeInfo
{
    private readonly HashSet<string> _files = new();
    private readonly HashSet<string> _changedFiles = new();
    public ulong Bytes { get; set; }
    public uint TotalFileCount { get; private set; }
    public uint AddedFileCount { get; set; }
    public uint DeletedFileCount { get; set; }
    public uint ChangedFileCount => (uint)(_changedFiles.Count + DeletedFileCount);

    public void RegisterFile(string filename)
    {
        TotalFileCount++;
        _files.Add(filename);
    }
    
    public void RegisterChangedFile(string filename)
    {
        _changedFiles.Add(filename);
    }

    public IEnumerable<string> Files => _files;
    public IEnumerable<string> ModifiedFiles => _changedFiles;

    public bool Contains(string filename) => _files.Contains(filename);
}