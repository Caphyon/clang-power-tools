using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClangPowerTools.MVVM.Command
{
  public class RelayCommand : ICommand
  {
    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }

    private readonly Action action;

    private Func<bool> canExecute;
    public RelayCommand(Action action, Func<bool> canExecute = null)
    {
      this.action = action;
      this.canExecute = canExecute;
    }
    public bool CanExecute(object parameter)
    {
      return canExecute == null ? true : canExecute.Invoke();
    }
    public void Execute(object parameter)
    {
      action.Invoke();
    }
  }
}
