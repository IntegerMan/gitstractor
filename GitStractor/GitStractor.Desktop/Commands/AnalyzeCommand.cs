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
    private readonly AppViewModel _vm;

    public AnalyzeCommand(AppViewModel vm)
    {
        _vm = vm;
    }

    public override void Execute(object parameter)
    {
        Dispatcher uiThread = Dispatcher.CurrentDispatcher;
        
        _vm.ShowAnalyze = false;
        _vm.BusyText = "Analyzing...";

        Task.Run(() =>
        {
            try
            {
                GitExtractionOptions options = GitExtractionOptions.BuildFileOptions(_vm.RepositoryPath);
                options.ProgressListener = this;

                using (GitDataExtractor extractor = new(options))
                {
                    extractor.ExtractInformation();
                }
            }
            catch (Exception ex)
            {
                uiThread.BeginInvoke(() =>
                {
                    _vm.BusyText = null;
                    _vm.ShowAnalyze = true;
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
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            _vm.ShowAnalyze = false;
            _vm.BusyText = busyText;
        });
    }

    public void UpdateProgress(double percentComplete, string statusText)
    {
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            _vm.BusyProgress = percentComplete;
            _vm.BusyText = statusText;
        });
    }

    public void Completed()
    {
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            _vm.BusyText = null;
        });
    }
}