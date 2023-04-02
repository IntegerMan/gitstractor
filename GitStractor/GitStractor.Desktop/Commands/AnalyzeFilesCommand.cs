using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitStractor.Desktop.ViewModels;
using Telerik.Windows.Controls;

namespace GitStractor.Desktop.Commands;

public class AnalyzeFilesCommand : CommandBase
{
    private readonly AppViewModel _vm;

    public AnalyzeFilesCommand(AppViewModel vm)
    {
        _vm = vm;
    }

    public override void Execute(object parameter)
    {
        string directory = _vm.AnalysisVM.RepositoryPath;

        if (string.IsNullOrWhiteSpace(directory))
        {
            RadWindow.Alert(new DialogParameters()
            {
                Header = $"Could Not Analyze Repository",
                Content = "Please select a repository for analysis first",
            });
        }
        else
        {
            _vm.ShowResults = true;
        }
    }
}