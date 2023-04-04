using System;
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
    private readonly List<CommitData> _commits = new();

    public ReportViewModel(AppViewModel appVM, string csvFilePath)
    {
        _appVm = appVM;

        string commitPath = Path.Combine(csvFilePath, "Commits.csv");

        _commits = CommitsCsvReader.ReadCommits(commitPath).ToList();
    }

    public IEnumerable<CommitData> Commits => _commits;
}