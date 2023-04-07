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

    public IEnumerable<TreeMapNode> FileCommits
    {
        get
        {
            List<TreeMapNode> nodes = new();

            IEnumerable<IGrouping<string, FileCommitData>> commits = _fileCommits.GroupBy(c => c.FilePath);
            double maxCommits = commits.Max(g => g.Count());
            double minCommits = commits.Min(g => g.Count());

            _files.ForEach(f =>
            {
                int numCommits = _fileCommits.Count(c => c.FilePath == f.FilePath);
                TreeMapNode node = new()
                {
                    Value = f.Lines,
                    ColorValue = (numCommits - minCommits) / maxCommits,
                    ToolTip = f.Filename + " (" + f.Lines + " lines, " + numCommits + " commits)",
                    Label = f.FilePath,
                };

                nodes.Add(node);
            });

            return nodes;
        }
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