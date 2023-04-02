using GitStractor.Desktop.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace GitStractor.Desktop.ViewModels
{
    public class AppViewModel : ViewModelBase
    {
        private bool _showAnalyze;

        private string _repoPath = @"C:\Dev\GitStractor";
        private string _busyText = null;

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

        public AppViewModel() {
            ShowAboutInfoCommand = new ShowAboutInfoCommand(this);
            ShowAnalyzeCommand = new ShowAnalyzeCommand(this);
            AnalyzeCommand = new AnalyzeCommand(this);
            ShowWelcomeCommand = new ShowWelcomeCommand(this);
            ExitCommand = new ExitCommand();
            NotImplementedCommand = new NotImplementedCommand();
        }

        public string AppName => "GitStractor";
        public string Author => "Matt Eland";
        public string Title => $"{AppName} by {Author}";
        public string Version => "Development Preview";
        public string Status => $"{AppName} Version";

        public CommandBase AnalyzeCommand { get; }
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
                    OnPropertyChanged(nameof(ShowWelcome));
                    OnPropertyChanged(nameof(IsBusy));
                }
            }
        }

        public bool IsBusyIndeterminent => true;

        public double BusyProgress => 42;

        public bool IsBusy => !string.IsNullOrEmpty(_busyText);
    }
}
