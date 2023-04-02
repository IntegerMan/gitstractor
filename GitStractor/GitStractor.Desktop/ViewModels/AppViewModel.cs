using GitStractor.Desktop.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Telerik.Windows.Controls;

namespace GitStractor.Desktop.ViewModels;

public class AppViewModel : ViewModelBase
{
    private bool _showAnalyze;
    private bool _showResults;

    private string _busyText = null;
    private double _busyProgress;

    public AppViewModel() {
        UIThread = Dispatcher.CurrentDispatcher;
        
        ShowAboutInfoCommand = new ShowAboutInfoCommand(this);
        ShowAnalyzeCommand = new ShowAnalyzeCommand(this);
        ShowWelcomeCommand = new ShowWelcomeCommand(this);
        OpenAnalysisCommand = new AnalyzeFilesCommand(this);
        ExitCommand = new ExitCommand();
        NotImplementedCommand = new NotImplementedCommand();

        AnalysisVM = new AnalyzeViewModel(this);
    }

    public AnalyzeViewModel AnalysisVM { get; set; }

    public string AppName => "GitStractor";
    public string Author => "Matt Eland";
    public string Title => $"{AppName} by {Author}";
    public string Version => "Development Preview";
    public string Status => BusyText ?? $"{AppName} Version";

    public CommandBase OpenAnalysisCommand { get; set; }
    public CommandBase ShowAboutInfoCommand { get; }
    public CommandBase ShowAnalyzeCommand { get; }
    public CommandBase ShowWelcomeCommand { get; }
    public CommandBase ExitCommand { get; }
    public CommandBase NotImplementedCommand { get; }

    public bool ShowWelcome => !ShowAnalyze && !IsBusy && !HasAnalysis && !ShowResults;

    private bool _hasAnalysis;
    public bool HasAnalysis
    {
        get => _hasAnalysis;
        set
        {
            if (_hasAnalysis != value)
            {
                _hasAnalysis = value;
                NotifyViewsChanged();
            }
        }
    }

    public bool ShowAnalyze
    {
        get => _showAnalyze;
        set
        {
            if (_showAnalyze != value)
            {
                _showAnalyze = value;
                _showResults = false;
                NotifyViewsChanged();
            }
        }
    }

    private void NotifyViewsChanged()
    {
        OnPropertyChanged(nameof(ShowAnalyze));
        OnPropertyChanged(nameof(ShowWelcome));
        OnPropertyChanged(nameof(ShowResults));
        OnPropertyChanged(nameof(HasAnalysis));
        OnPropertyChanged(nameof(IsBusy));
        OnPropertyChanged(nameof(Status));
    }

    public bool ShowResults
    {
        get => _showResults;
        set
        {
            if (_showResults != value)
            {
                _showResults = value;
                _showAnalyze = false;
                NotifyViewsChanged();
            }
        }
    }

    public string BusyText
    {
        get => _busyText;
        set
        {
            if (_busyText != value)
            {
                _busyText = value;
                NotifyViewsChanged();
            }
        }
    }

    public bool IsBusyIndeterminent => false;

    public double BusyProgress
    {
        get => _busyProgress;
        set
        {
            if (_busyProgress != value)
            {
                _busyProgress = value;
                OnPropertyChanged(nameof(BusyProgress));
            }
        }
    }

    public bool IsBusy => !string.IsNullOrEmpty(_busyText);
    public Dispatcher UIThread { get; }
}