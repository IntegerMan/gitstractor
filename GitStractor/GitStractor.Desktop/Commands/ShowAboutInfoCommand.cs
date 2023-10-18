using GitStractor.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace GitStractor.Desktop.Commands;

public class ShowAboutInfoCommand : CommandBase
{
    private readonly AppViewModel _vm;

    public ShowAboutInfoCommand(AppViewModel vm)
    {
        this._vm = vm;
    }

    public override void Execute(object parameter)
    {
        RadWindow.Alert(new DialogParameters()
        {
            Header = $"About {_vm.AppName}",
            Content = $"{_vm.Version} by {_vm.Author}",
        });
    }
}
