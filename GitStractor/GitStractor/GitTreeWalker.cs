using GitStractor.GitObservers;
using GitStractor.Model;
using Microsoft.Extensions.Logging;

namespace GitStractor;

public class GitTreeWalker {

    public ILogger<GitTreeWalker> Log { get; set; }

    public GitTreeWalker(ILogger<GitTreeWalker> log) {
        Log = log;
    }

    public GitTreeInfo WalkCommitTree(Commit commit, Repository repo, List<IGitObserver> observers) {
        GitTreeInfo treeInfo = new();

        TreeChanges changes = repo.Diff.Compare<TreeChanges>(commit.Parents.FirstOrDefault()?.Tree, commit.Tree);
        PatchStats patchStats = repo.Diff.Compare<PatchStats>(commit.Parents.FirstOrDefault()?.Tree, commit.Tree);

        foreach (TreeEntryChanges change in changes) {
            ProcessTreeChange(commit, observers, treeInfo, change, patchStats[change.Path]);
        }

        if (commit == repo.Head.Tip) {
            WalkTree(commit, treeInfo, observers);
        }

        return treeInfo;
    }

    private static void ProcessTreeChange(Commit commit, List<IGitObserver> observers, GitTreeInfo treeInfo, TreeEntryChanges change, ContentChangeStats stats) {
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
            Lines = 0,
            Bytes = 0,
            State = state
        };
        treeInfo.Register(fileInfo);
        observers.ForEach(o => o.OnProcessingFile(fileInfo, commit.Sha));
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
        // Using a Queue here is more efficient with the call stack for deep hierarchies than using recursion
        Queue<TreeEntry> entries = new(commit.Tree);

        while (entries.Count > 0) {
            TreeEntry treeEntry = entries.Dequeue();

            switch (treeEntry.Mode) {
                case Mode.NonExecutableFile:
                case Mode.NonExecutableGroupWritableFile:
                case Mode.ExecutableFile:
                    Blob blob = (Blob)treeEntry.Target;
                    ProcessFile(commit, treeInfo, blob, treeEntry.Path, observers);
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

    private static void ProcessFile(Commit commit, GitTreeInfo treeInfo, Blob blob, string path, List<IGitObserver> observers) {
        (int lines, ulong bytes) = CountLinesAndBytes(blob);

        RepositoryFileInfo fileInfo = new() {
            Sha = blob.Id.Sha,
            Commit = commit.Sha,
            Path = path,
            OldPath = null,
            Lines = lines,
            LinesDelta = 0,
            Bytes = bytes,
            State = FileState.Final
        };

        treeInfo.Register(fileInfo);

        observers.ForEach(o => o.OnProcessingFile(fileInfo, commit.Sha));
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
