using GitStractor.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitStractor;

public class GitTreeWalker {

    public ILogger<GitTreeWalker> Log { get; set; }
    private readonly Dictionary<string, string> _pathShas = new();
    private readonly Dictionary<string, RepositoryFileInfo> _fileInfo = new();

    public GitTreeWalker(ILogger<GitTreeWalker> log) {
        Log = log;
    }

    private readonly Dictionary<string, GitTreeInfo> _trees = new();

    public GitTreeInfo WalkCommitTree(Commit commit, bool isLastCommit) {
        GitTreeInfo treeInfo = new();
        _trees[commit.Tree.Sha] = treeInfo;

        // Walk the commit tree to get file information
        WalkTree(commit, commit.Tree, treeInfo, isLastCommit);

        // Get what we know about the parent commit this came from
        GitTreeInfo? parentTree = commit.Parents.Any() ? _trees[commit.Parents.First().Tree.Sha] : null;

        // Detect any Deleted Files
        if (parentTree != null) {
            foreach (string path in parentTree.Files) {
                if (!treeInfo.Contains(path)) {
                    RepositoryFileInfo file = parentTree.Find(path)!;
                    Log.LogDebug($"Detected deleted file {file.Path}");
                    //_options!.FileWriter.WriteFile(file.AsDeletedFile(commit.Sha, commit.Author.When.UtcDateTime));
                }
            }
        }

        return treeInfo;
    }

    private void WalkTree(Commit commit, Tree tree, GitTreeInfo treeInfo, bool isLast) {
        foreach (TreeEntry treeEntry in tree) {
            if (treeEntry.TargetType == TreeEntryTargetType.Blob) {
                // Ignore the file if it's not an extension we care about
                FileInfo info = new(treeEntry.Path);
                if (true) { //_options!.FileMatchesFilter(info.Extension)) {
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

                    //_options.FileWriter.WriteFile(fileInfo);
                    // At the very end of the analysis, we want to write out the final state of the file
                    if (isLast) {
                        //_options.FileWriter.WriteFile(fileInfo.AsFinalVersion());
                    }
                }
            } else if (treeEntry.TargetType == TreeEntryTargetType.Tree) {
                Tree subTree = (Tree)treeEntry.Target;

                WalkTree(commit, subTree, treeInfo, isLast);
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
