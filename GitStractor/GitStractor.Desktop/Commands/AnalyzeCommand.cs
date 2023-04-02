using GitStractor.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace GitStractor.Desktop.Commands;

public class AnalyzeCommand : CommandBase
{
    private readonly AppViewModel _vm;

    public AnalyzeCommand(AppViewModel vm)
    {
        _vm = vm;
    }

    public override void Execute(object parameter)
    {
        _vm.ShowAnalyze = false;
        _vm.BusyText = $"Analyzing '{_vm.RepositoryPath}' ...";
    }
}