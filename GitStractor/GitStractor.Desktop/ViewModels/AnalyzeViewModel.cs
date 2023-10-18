using System.Windows.Threading;
using GitStractor.Desktop.Commands;
using Telerik.Windows.Controls;

namespace GitStractor.Desktop.ViewModels;

public class AnalyzeViewModel : ViewModelBase
{
    private readonly AppViewModel _appVm;

    public AnalyzeViewModel(AppViewModel appVM)
    {
        _appVm = appVM;
        AnalyzeCommand = new AnalyzeCommand(this);
        BackCommand = new ShowWelcomeCommand(appVM);
    }

    private string _repoPath = @"C:\Dev\GitStractor";

    private bool _isAnalyzing;
    private bool _analysisComplete;

    public string RepositoryPath
    {
        get => _repoPath;
        set
        {
            if (_repoPath != value)
            {
                _repoPath = value;
                base.OnPropertyChanged(nameof(RepositoryPath));
                AnalyzeCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsAnalyzing
    {
        get => _isAnalyzing;
        set
        {
            if (_isAnalyzing != value)
            {
                _isAnalyzing = value;
                base.OnPropertyChanged(nameof(IsAnalyzing));
                AnalyzeCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public CommandBase AnalyzeCommand { get; }
    public CommandBase BackCommand { get; }

    public bool AnalysisComplete
    {
        get => _analysisComplete;
        set
        {
            if (_analysisComplete != value)
            {
                _analysisComplete = value;
                OnPropertyChanged(nameof(AnalysisComplete));
                _appVm.HasAnalysis = value;
            }
        }
    }

    public Dispatcher UIThread => _appVm.UIThread;

    public void SetBusy(string busyText, double percentComplete)
    {
        _appVm.BusyText = busyText;
        _appVm.BusyProgress = percentComplete;
    }

    public void SetNotBusy()
    {
        _appVm.BusyText = null;
        _appVm.ShowAnalyze = true;
    }

    public void CompletedAnalysis()
    {
        SetNotBusy();
        IsAnalyzing = false;
        AnalysisComplete = true;
        BackCommand.Execute(this);
    }
}