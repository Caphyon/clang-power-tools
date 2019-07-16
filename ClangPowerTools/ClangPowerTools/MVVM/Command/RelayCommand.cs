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

    private readonly Action _action;

    private Func<bool> _canExecute;
    public RelayCommand(Action action, Func<bool> canExecute = null)
    {
      _action = action;
      _canExecute = canExecute;
    }
    public bool CanExecute(object parameter)
    {
      return _canExecute == null ? true : _canExecute.Invoke();
    }
    public void Execute(object parameter)
    {
      _action.Invoke();
    }
  }
}
