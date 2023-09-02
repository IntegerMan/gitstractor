using GitStractor.Model;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GitStractor;

public class GitTreeWalker {

    public ILogger<GitTreeWalker> Log { get; set; }
    private readonly Dictionary<string, string> _pathShas = new();
    private readonly Dictionary<string, RepositoryFileInfo> _fileInfo = new();
    private readonly Dictionary<string, Tree> _trees = new();

    public GitTreeWalker(ILogger<GitTreeWalker> log) {
        Log = log;
    }


    public GitTreeInfo WalkCommitTree(Commit commit, Repository repo, List<IGitObserver> observers) {
        GitTreeInfo treeInfo = new();
        _trees[commit.Sha] = commit.Tree;

        TreeChanges changes = repo.Diff.Compare<TreeChanges>(commit.Parents.FirstOrDefault()?.Tree, commit.Tree);

        foreach (TreeEntryChanges change in changes) {
            ChangeKind kind = change.Status;
            FileState state = GetFileState(kind);

            RepositoryFileInfo fileInfo = new() {
                Sha = change.Oid.Sha,
                Commit = commit.Sha,
                Path = change.Path,
                OldPath = change.OldPath,
                Lines = 0, // TODO
                LinesDelta = 0, // TODO
                Bytes = 0,
                State = state
            };
            treeInfo.Register(fileInfo);
            observers.ForEach(o => o.OnProcessingFile(fileInfo, commit.Sha));
        }

        if (commit == repo.Head.Tip) {
            // TODO: Loop over the tree and report the final file structure
        }

        return treeInfo;
    }

    private static FileState GetFileState(ChangeKind kind)
        => kind switch {
            ChangeKind.Added => FileState.Added,
            ChangeKind.Unmodified => FileState.Unmodified,
            ChangeKind.Deleted => FileState.Deleted,
            ChangeKind.Modified => FileState.Modified,
            ChangeKind.Renamed => FileState.Renamed,
            ChangeKind.TypeChanged => FileState.TypeChanged,
            ChangeKind.Conflicted => FileState.Conflicted,
            ChangeKind.Unreadable => FileState.Unreadable, // What is this?
            ChangeKind.Copied => FileState.Copied, // What is this?
            ChangeKind.Ignored => FileState.Ignored, // What is this?
            _ => FileState.Untracked,
        };

    private void WalkTree(Commit commit, Tree tree, GitTreeInfo treeInfo, List<IGitObserver> observers) {
        Queue<TreeEntry> entries = new(tree);

        while (entries.Count > 0) {
            TreeEntry treeEntry = entries.Dequeue();

            switch (treeEntry.Mode) {
                case Mode.NonExecutableFile:
                case Mode.NonExecutableGroupWritableFile:
                    _fileInfo.TryGetValue(treeEntry.Path, out RepositoryFileInfo? oldInfo);

                    RepositoryFileInfo fileInfo;
                    if (oldInfo != null && treeEntry.Target.Sha == oldInfo.Sha) {
                        fileInfo = oldInfo.AsUnchanged();
                    } else {
                        fileInfo = BuildFileInfo(commit, treeEntry, oldInfo);
                    }
                    _fileInfo[treeEntry.Path] = fileInfo;

                    // Each tree consists of ALL files, including those who didn't change at all. When the contents of a
                    // file changes, its SHA changes. So, we can use the SHA of the file to determine if it changed.
                    // If the SHA is the same, we shouldn't log the file as being part of this commit, though there may
                    // be analysis value of having a full log of all files as of any given commit in the system
                    string fileLower = treeEntry.Path.ToLowerInvariant();
                    fileInfo.State = DetermineFileState(fileLower, treeEntry);

                    treeInfo.Register(fileInfo);

                    // Add or update our entry for the file's path
                    _pathShas[fileLower] = treeEntry.Target.Sha;

                    observers.ForEach(o => o.OnProcessingFile(fileInfo, commit.Sha));
                    break;

                case Mode.Directory:
                    // Avoid nested recursion by appending to a queue
                    foreach (TreeEntry nestedEntry in (Tree)treeEntry.Target) {
                        entries.Enqueue(nestedEntry);
                    }
                    break;

                case Mode.SymbolicLink:
                    throw new NotSupportedException("Git repositories with Symbolic Links are not yet supported by GitStractor");
                case Mode.GitLink:
                    throw new NotSupportedException("Git repositories with GitLinks are not yet supported by GitStractor");
                case Mode.ExecutableFile:
                    // Maybe this is supportable as a standard file?
                    throw new NotSupportedException("Git repositories with Executable Files are not yet supported by GitStractor");
            }
        }
    }

    public void Clear() {
        _trees.Clear();
        _pathShas.Clear();
        _fileInfo.Clear();
    }

    private FileState DetermineFileState(string fileLower, TreeEntry treeEntry) {
        if (!_pathShas.TryGetValue(fileLower, out string? value)) {
            // If we didn't have the path before, this is an added file so let's mark it as an added file
            return FileState.Added;
        }

        return value == treeEntry.Target.Sha
            ? FileState.Unmodified
            : FileState.Modified;
    }

    private static RepositoryFileInfo BuildFileInfo(Commit commit, TreeEntry treeEntry, RepositoryFileInfo? oldInfo) {
        Blob blob = (Blob)treeEntry.Target;

        int lines = CountLinesInFile(blob);

        int linesDelta = lines - oldInfo?.Lines ?? lines;

        return new RepositoryFileInfo() {
            Name = treeEntry.Name,
            Path = treeEntry.Path,
            Sha = blob.Id.Sha,
            Lines = lines,
            LinesDelta = linesDelta,
            Bytes = (ulong)blob.Size,
            Commit = commit.Id.Sha,
            CreatedDateUtc = commit.Author.When.UtcDateTime,
        };
    }

    private static int CountLinesInFile(Blob blob) {
        int lines = 0;
        using var reader = new StreamReader(blob.GetContentStream());

        while (!reader.EndOfStream) {
            _ = reader.ReadLine();
            lines++;
        }

        return lines;
    }

}
