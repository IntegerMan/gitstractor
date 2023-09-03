using GitStractor.GitObservers;
using GitStractor.Model;
using Microsoft.Extensions.Logging;
using System.IO;

namespace GitStractor;

public class GitTreeWalker {

    public ILogger<GitTreeWalker> Log { get; set; }

    public GitTreeWalker(ILogger<GitTreeWalker> log) {
        Log = log;
    }

    public GitTreeInfo WalkCommitTree(Commit commit, CommitInfo commitInfo, Repository repo, List<IGitObserver> observers) {
        GitTreeInfo treeInfo = new();

        TreeChanges changes = repo.Diff.Compare<TreeChanges>(commit.Parents.FirstOrDefault()?.Tree, commit.Tree);
        PatchStats patchStats = repo.Diff.Compare<PatchStats>(commit.Parents.FirstOrDefault()?.Tree, commit.Tree);

        foreach (TreeEntryChanges change in changes) {
            RepositoryFileInfo fileInfo = ProcessTreeChange(commit, treeInfo, change, patchStats[change.Path]);
            observers.ForEach(o => o.OnProcessingFile(fileInfo, commitInfo));
        }

        if (commit == repo.Head.Tip) {
            WalkTree(commit, commitInfo, treeInfo, observers);
        }

        return treeInfo;
    }

    private static RepositoryFileInfo ProcessTreeChange(Commit commit, GitTreeInfo treeInfo, TreeEntryChanges change, ContentChangeStats stats) {
        ChangeKind kind = change.Status;
        FileState state = GetFileState(kind);

        RepositoryFileInfo fileInfo = new() {
            Sha = change.Oid.Sha,
            Commit = commit.Sha,
            Path = change.Path,
            OldPath = change.Path == change.OldPath ? null : change.OldPath,
            LinesDeleted = stats.LinesDeleted,
            LinesAdded = stats.LinesAdded,
            LinesDelta = stats.LinesAdded - stats.LinesDeleted,
            Lines = 0, // TODO
            State = state
        };
        treeInfo.Register(fileInfo);
        return fileInfo;
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

    private void WalkTree(Commit commit, CommitInfo commitInfo, GitTreeInfo treeInfo, List<IGitObserver> observers) {
        // Using a Queue here is more efficient with the call stack for deep hierarchies than using recursion
        Queue<TreeEntry> entries = new(commit.Tree);

        while (entries.Count > 0) {
            TreeEntry treeEntry = entries.Dequeue();

            switch (treeEntry.Mode) {
                case Mode.NonExecutableFile:
                case Mode.NonExecutableGroupWritableFile:
                case Mode.ExecutableFile:
                    Blob blob = (Blob)treeEntry.Target;
                    RepositoryFileInfo fileInfo = ProcessFile(commit, treeInfo, blob, treeEntry.Path);
                    observers.ForEach(o => o.OnProcessingFile(fileInfo, commitInfo));
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
            }
        }
    }

    private static RepositoryFileInfo ProcessFile(Commit commit, GitTreeInfo treeInfo, Blob blob, string path) {
        int lines = CountLines(blob);

        RepositoryFileInfo fileInfo = new() {
            Sha = blob.Id.Sha,
            Commit = commit.Sha,
            Path = path,
            OldPath = null,
            Lines = lines,
            LinesDelta = 0,
            State = FileState.Final
        };

        treeInfo.Register(fileInfo);
        return fileInfo;
    }

    public static int CountLines(Blob blob) {
        int lines = 0;
        using var stream = blob.GetContentStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);
        while (!reader.EndOfStream) {
            string? line = reader.ReadLine();
            lines++;
        }
        return lines;
    }

}
