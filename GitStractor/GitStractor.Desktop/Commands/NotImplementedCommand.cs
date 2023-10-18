using GitStractor.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace GitStractor.Desktop.Commands;

public class NotImplementedCommand : CommandBase
{
    public override void Execute(object parameter)
    {
        RadWindow.Alert(new DialogParameters()
        {
            Header = "Not Implemented",
            Content = "This feature is not yet implemented",
        });
    }
}