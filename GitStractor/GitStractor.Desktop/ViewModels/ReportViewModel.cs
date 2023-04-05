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
    private readonly List<CommitData> _commits = new();

    public ReportViewModel(AppViewModel appVM, string csvFilePath)
    {
        _appVm = appVM;

        string commitPath = Path.Combine(csvFilePath, "Commits.csv");

        _commits = CommitsCsvReader.ReadCommits(commitPath).OrderBy(c => c.AuthorDateUTC).ToList();
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