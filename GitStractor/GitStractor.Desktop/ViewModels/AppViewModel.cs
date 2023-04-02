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

    private string _busyText = null;
    private double _busyProgress;

    public AppViewModel() {
        UIThread = Dispatcher.CurrentDispatcher;
        
        ShowAboutInfoCommand = new ShowAboutInfoCommand(this);
        ShowAnalyzeCommand = new ShowAnalyzeCommand(this);
        ShowWelcomeCommand = new ShowWelcomeCommand(this);
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

    public CommandBase ShowAboutInfoCommand { get; }
    public CommandBase ShowAnalyzeCommand { get; }
    public CommandBase ShowWelcomeCommand { get; }
    public CommandBase ExitCommand { get; }
    public CommandBase NotImplementedCommand { get; }

    public bool ShowWelcome => !ShowAnalyze && !IsBusy && !HasAnalysis;
    public bool HasAnalysis => false;

    public bool ShowAnalyze
    {
        get => _showAnalyze;
        set
        {
            if (_showAnalyze != value)
            {
                _showAnalyze = value;
                base.OnPropertyChanged(nameof(ShowAnalyze));
                base.OnPropertyChanged(nameof(ShowWelcome));
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
                OnPropertyChanged(nameof(BusyText));
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(ShowWelcome));
                OnPropertyChanged(nameof(IsBusy));
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