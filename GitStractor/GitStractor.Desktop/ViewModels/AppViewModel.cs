using GitStractor.Desktop.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace GitStractor.Desktop.ViewModels
{
    public class AppViewModel : ViewModelBase
    {
        public AppViewModel() {
            ShowAboutInfoCommand = new ShowAboutInfoCommand(this);
        }

        public string AppName => "GitStractor";
        public string Author => "Matt Eland";
        public string Title => $"{AppName} by {Author}";
        public string Version => "Development Preview";
        public string Status => $"{AppName} Version";

        public ICommand ShowAboutInfoCommand { get; }
        public ICommand ExitCommand { get; } = new ExitCommand();
        public ICommand NotImplementedCommand { get; } = new NotImplementedCommand();
    }
}
