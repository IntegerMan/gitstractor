using GitStractor.GitObservers;
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
                OldPath = change.Path == change.OldPath ? null : change.OldPath,
                Lines = 0, // TODO
                LinesDelta = 0, // TODO
                Bytes = 0,
                State = state
            };
            treeInfo.Register(fileInfo);
            observers.ForEach(o => o.OnProcessingFile(fileInfo, commit.Sha));
        }

        if (commit == repo.Head.Tip) {
            WalkTree(commit, treeInfo, observers);
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

    private void WalkTree(Commit commit, GitTreeInfo treeInfo, List<IGitObserver> observers) {
        Queue<TreeEntry> entries = new(commit.Tree);

        while (entries.Count > 0) {
            TreeEntry treeEntry = entries.Dequeue();

            switch (treeEntry.Mode) {
                case Mode.NonExecutableFile:
                case Mode.NonExecutableGroupWritableFile:
                    Blob blob = (Blob)treeEntry.Target;

                    (int lines, ulong bytes) = CountLinesAndBytes(blob);

                    RepositoryFileInfo fileInfo = new() {
                        Sha = blob.Id.Sha,
                        Commit = commit.Sha,
                        Path = treeEntry.Path,
                        OldPath = null,
                        Lines = lines,
                        LinesDelta = 0,
                        Bytes = bytes,
                        State = FileState.Final
                    };

                    treeInfo.Register(fileInfo);

                    observers.ForEach(o => o.OnProcessingFile(fileInfo, commit.Sha));
                    break;

                case Mode.Directory:
                    // Avoid nested recursion by appending to a queue
                    foreach (TreeEntry nestedEntry in (Tree)treeEntry.Target) {
                        entries.Enqueue(nestedEntry);
                    }
                    break;

                case Mode.SymbolicLink:
                    Log.LogWarning("Repository contained a Symbolic Link which is not currently supported by GitStractor. Some entries may be missing");
                    break;
                case Mode.GitLink:
                    Log.LogWarning("Repository contained a GitLink which is not currently supported by GitStractor. Some entries may be missing");
                    break;
                case Mode.ExecutableFile:
                    // Maybe this is supportable as a standard file?
                    Log.LogWarning("Repository contained an executable file which is not currently supported by GitStractor. Some entries may be missing");
                    break;
            }
        }
    }

    public void Clear() {
        _trees.Clear();
        _pathShas.Clear();
        _fileInfo.Clear();
    }

    public static (int lines, ulong bytes) CountLinesAndBytes(Blob blob) {
        int lines = 0;
        ulong bytes = 0;
        using var stream = blob.GetContentStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);
        while (!reader.EndOfStream) {
            string? line = reader.ReadLine();
            lines++;
            if (line != null) {
                bytes += (ulong)(Encoding.UTF8.GetByteCount(line) + Encoding.UTF8.GetByteCount(Environment.NewLine));
            }
        }
        return (lines, bytes);
    }

}
