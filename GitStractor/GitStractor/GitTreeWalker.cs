using GitStractor.GitObservers;
using GitStractor.Model;
using Microsoft.Extensions.Logging;

namespace GitStractor;

public class GitTreeWalker {

    public ILogger<GitTreeWalker> Log { get; set; }

    public GitTreeWalker(ILogger<GitTreeWalker> log) {
        Log = log;
    }

    public CommitInfo WalkCommitTree(Commit commit, CommitInfo commitInfo, Repository repo, List<IGitObserver> observers, IEnumerable<string> ignorePatterns) {
        TreeChanges changes = repo.Diff.Compare<TreeChanges>(commit.Parents.FirstOrDefault()?.Tree, commit.Tree);
        PatchStats patchStats = repo.Diff.Compare<PatchStats>(commit.Parents.FirstOrDefault()?.Tree, commit.Tree);

        foreach (TreeEntryChanges change in changes) {
            RepositoryFileInfo fileInfo = ProcessTreeChange(commit, change, patchStats[change.Path]);

            commitInfo.TotalFiles++;

            // If it is a file we want to ignore, don't do anything with the file. We DO want it to count towards the total file number for the commit, though, so that remains accurate
            if (FileMatchesPattern(fileInfo, ignorePatterns))
                continue;

            commitInfo.Add(fileInfo);

            switch (fileInfo.State) {
                case FileState.Added:
                    commitInfo.FilesAdded++;
                    commitInfo.TotalLines += fileInfo.Lines;
                    break;

                case FileState.Deleted:
                    commitInfo.FilesDeleted++;
                    break;

                case FileState.Modified:
                case FileState.Renamed:
                case FileState.Conflicted:
                    commitInfo.FilesModified++;
                    commitInfo.TotalLines += fileInfo.Lines;
                    break;
            }

            commitInfo.LinesAdded += fileInfo.LinesAdded;
            commitInfo.LinesDeleted += fileInfo.LinesDeleted;

            observers.ForEach(o => o.OnProcessingFile(fileInfo, commitInfo));
        }

        if (commit == repo.Head.Tip) {
            WalkTree(commit, commitInfo, observers);
        }

        return commitInfo;
    }

    private static bool FileMatchesPattern(RepositoryFileInfo fileInfo, IEnumerable<string> ignorePatterns)
        => ignorePatterns.Any(p => fileInfo.Extension.Equals(p, StringComparison.OrdinalIgnoreCase) || fileInfo.Path.EndsWith(p, StringComparison.OrdinalIgnoreCase));

    private static RepositoryFileInfo ProcessTreeChange(Commit commit, TreeEntryChanges change, ContentChangeStats stats) {
        ChangeKind kind = change.Status;
        FileState state = GetFileState(kind);

        string path = change.Path;
        Blob? blob = FindBlobInTree(commit.Tree, path);

        return new RepositoryFileInfo() {
            Sha = change.Oid.Sha,
            Commit = commit.Sha,
            Path = change.Path,
            OldPath = change.Path == change.OldPath ? null : change.OldPath,
            LinesDeleted = stats.LinesDeleted,
            LinesAdded = stats.LinesAdded,
            Lines = blob == null ? 0 : CountLines(blob),
            State = state
        };
    }

    private static Blob? FindBlobInTree(Tree tree, string path) {
        Queue<TreeEntry> entries = new(tree);
        while (entries.Count > 0) {
            TreeEntry entry = entries.Dequeue();

            if (entry.TargetType == TreeEntryTargetType.Blob && entry.Path == path) {
                return (Blob)entry.Target;
            }

            if (entry.TargetType == TreeEntryTargetType.Tree) {
                foreach (TreeEntry nestedEntry in (Tree)entry.Target) {
                    entries.Enqueue(nestedEntry);
                }
            }
        }
        return null;
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

    private void WalkTree(Commit commit, CommitInfo commitInfo, List<IGitObserver> observers) {
        // Using a Queue here is more efficient with the call stack for deep hierarchies than using recursion
        Queue<TreeEntry> entries = new(commit.Tree);

        while (entries.Count > 0) {
            TreeEntry treeEntry = entries.Dequeue();

            switch (treeEntry.Mode) {
                case Mode.NonExecutableFile:
                case Mode.NonExecutableGroupWritableFile:
                case Mode.ExecutableFile:
                    Blob blob = (Blob)treeEntry.Target;
                    RepositoryFileInfo fileInfo = ProcessFile(commit, blob, treeEntry.Path);
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

    private static RepositoryFileInfo ProcessFile(Commit commit, Blob blob, string path)
        => new() {
            Sha = blob.Id.Sha,
            Commit = commit.Sha,
            Path = path,
            OldPath = null,
            Lines = CountLines(blob),
            State = FileState.Final
        };

    public static int CountLines(Blob blob) {
        using StreamReader reader = new(blob.GetContentStream(), Encoding.UTF8, false);
        int count = 0;
        while (reader.ReadLine() != null) {
            count++;
        }
        return count;
    }

}
