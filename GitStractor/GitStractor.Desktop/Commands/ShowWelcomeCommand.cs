﻿using GitStractor.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace GitStractor.Desktop.Commands;

public class ShowWelcomeCommand : CommandBase
{
    private readonly AppViewModel _vm;

    public ShowWelcomeCommand(AppViewModel vm)
    {
        _vm = vm;
    }

    public override void Execute(object parameter)
    {
        _vm.ShowAnalyze = false;
    }
}