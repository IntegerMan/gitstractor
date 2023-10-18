using GitStractor.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Telerik.Windows.Controls;

namespace GitStractor.Desktop.Commands;

public class AnalyzeCommand : CommandBase, IProgressListener
{
    private readonly AnalyzeViewModel _vm;

    public AnalyzeCommand(AnalyzeViewModel vm)
    {
        _vm = vm;
    }

    public override bool CanExecute(object parameter)
    {
        return !_vm.IsAnalyzing && !string.IsNullOrWhiteSpace(_vm.RepositoryPath);
    }

    public override void Execute(object parameter)
    {
        _vm.IsAnalyzing = true;
        _vm.SetBusy("Analyzing...", 0);

        Task.Run(() =>
        {
            try
            {
                GitDataExtractor extractor = new(log: null, observers: null, new GitTreeWalker(log: null));
                extractor.ExtractInformation(repoPath: null, outputPath: null, ignorePatterns: Enumerable.Empty<string>());
            }
            catch (Exception ex)
            {
                _vm.UIThread.BeginInvoke(() =>
                {
                    _vm.SetNotBusy();
                    RadWindow.Alert(new DialogParameters()
                    {
                        Header = $"Could Not Analyze Repository",
                        Content = ex.Message,
                    });
                }, DispatcherPriority.Normal);
            }
        });
    }

    public void Started(string busyText)
    {
        _vm.UIThread.Invoke(() =>
        {
            _vm.SetBusy(busyText, 0);
        });
    }

    public void UpdateProgress(double percentComplete, string statusText)
    {
        _vm.UIThread.Invoke(() =>
        {
            _vm.SetBusy(statusText, percentComplete);
        });
    }

    public void Completed()
    {
        _vm.UIThread.Invoke(() =>
        {
            _vm.CompletedAnalysis();
        });
    }
}