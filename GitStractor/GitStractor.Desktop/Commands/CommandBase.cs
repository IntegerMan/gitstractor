using System;
using System.Windows.Input;

namespace GitStractor.Desktop.Commands;

public abstract class CommandBase : ICommand
{
    public virtual bool CanExecute(object parameter)
    {
        return true;
    }

    public abstract void Execute(object parameter);

    public event EventHandler CanExecuteChanged;

    protected void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}