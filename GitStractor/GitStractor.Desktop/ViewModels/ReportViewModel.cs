using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitStractor.Readers;
using Telerik.Windows.Controls;

namespace GitStractor.Desktop.ViewModels;

public class ReportViewModel : ViewModelBase
{
    private readonly AppViewModel _appVm;
    private readonly List<CommitData> _commits;
    private readonly List<FileCommitData> _fileCommits;
    private readonly List<FileData> _files;

    public ReportViewModel(AppViewModel appVM, string csvFilePath)
    {
        _appVm = appVM;

        string commitPath = Path.Combine(csvFilePath, "Commits.csv");
        string fileCommitPath = Path.Combine(csvFilePath, "FileCommits.csv");
        string filesPath = Path.Combine(csvFilePath, "FinalStructure.csv");

        _commits = CommitsCsvReader.ReadCommits(commitPath).OrderBy(c => c.AuthorDateUTC).ToList();
        _fileCommits = FileCsvReader.ReadFileCommits(fileCommitPath).OrderBy(c => c.AuthorDateUTC).ToList();
        _files = FileCsvReader.ReadFiles(filesPath).OrderBy(c => c.FilePath).ToList();
    }

    public IEnumerable<ITreeMapNode> FileCommits
    {
        get
        {
            List<ITreeMapNode> nodes = new();

            Dictionary<string, ParentTreeMapNode> rootDirectories = new();

            IEnumerable<IGrouping<string, FileCommitData>> commits = _fileCommits.GroupBy(c => c.FilePath);
            double maxCommits = commits.Max(g => g.Count());
            double minCommits = commits.Min(g => g.Count());

            // Loop over files by the length of their path. This ensures that the parent directories are handled before nested directories
            _files.OrderBy(f => f.FilePath.Length).ToList().ForEach(f =>
            {
                string[] parts = f.FilePath.Split('/');

                if (parts.Length <= 1)
                {
                    TreeMapNode node = GetTreeNodeFromFile(f, minCommits, maxCommits);

                    nodes.Add(node);
                }
                else
                {

                    ParentTreeMapNode node = null;
                    string directoryPath = string.Join('/', parts.Take(parts.Length - 1));
                    string directoryName = "";

                    // Find the nearest parent directory
                    int i = parts.Length - 1;
                    while (node == null && i > 0)
                    {
                        string partialPath = string.Join('/', parts.Take(i));

                        if (rootDirectories.ContainsKey(partialPath.ToLowerInvariant()))
                        {
                            node = rootDirectories[partialPath.ToLowerInvariant()];
                            directoryName = parts[i];
                        }

                        i--;
                    }
                    
                    if (node == null)
                    {
                        // This is a new root-level directory
                        node = new()
                        {
                            Label = directoryPath,
                            Key = directoryPath,
                        };
                        rootDirectories[directoryPath.ToLowerInvariant()] = node;
                        nodes.Add(node);
                    } 
                    else if (node.Key.ToLowerInvariant() != directoryPath.ToLowerInvariant())
                    {
                        // This is a new nested directory
                        ParentTreeMapNode subdir = new()
                        {
                            Label = parts[parts.Length - 2],
                            ToolTip = directoryPath,
                            Key = directoryPath,
                        };
                        rootDirectories[directoryPath.ToLowerInvariant()] = subdir;
                        node.Children.Add(subdir);

                        // The file should be added to the subdirectory
                        node = subdir;
                    }

                    node.Children.Add(GetTreeNodeFromFile(f, minCommits,maxCommits));
                }
            });

            return nodes;
        }
    }

    private TreeMapNode GetTreeNodeFromFile(FileData fileData, double minCommits, double maxCommits)
    {
        int numCommits = _fileCommits.Count(c => c.FilePath == fileData.FilePath);

        return new TreeMapNode()
        {
            Value = fileData.Lines,
            ColorValue = (numCommits - minCommits) / maxCommits,
            ToolTip = fileData.FilePath + " (" + fileData.Lines + " lines, " + numCommits + " commits)",
            Label = fileData.Filename,
        };
    }

    public IEnumerable<TreeMapNode> FileCommits2
    {
        get
        {
            List<TreeMapNode> nodes = new();
            
            _fileCommits.GroupBy(c => c.FilePath).ToList().ForEach(g =>
            {
                TreeMapNode node = new()
                {
                    Value = g.Count(),
                    Label = g.Key,
                    // TODO: Set Children
                };

                nodes.Add(node);
            });
            
            return nodes;
        }
    }

    public IEnumerable<CommitData> Commits => _commits;
    public IEnumerable AuthorsOverTime
    {
        get
        {
            IEnumerable<IGrouping<DateTime, CommitData>> authorsOverTime = _commits.GroupBy(c => c.AuthorDateUTC.Date);

            return authorsOverTime.Select(g =>
                new
                {
                    Date = g.Key,
                    Count = g.DistinctBy(c => c.AuthorEmail).Count()
                });
        }
    }

    public DateTime MaxDate => _commits.Max(c => c.AuthorDateUTC).AddMonths(1);
    public DateTime MinDate => _commits.Min(c => c.AuthorDateUTC).AddMonths(-1);
}