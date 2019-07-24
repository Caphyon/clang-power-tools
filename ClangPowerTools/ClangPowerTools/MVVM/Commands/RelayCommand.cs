using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClangPowerTools.MVVM.Commands
{
  public class RelayCommand : ICommand
  {
    #region Members
    private readonly Action action;
    private readonly Func<bool> canExecute;
    #endregion

    public RelayCommand(Action action, Func<bool> canExecute = null)
    {
      this.action = action;
      this.canExecute = canExecute;
    }

    #region Contructors
    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }
    #endregion


    #region Public Methods
    public bool CanExecute(object parameter)
    {
      return canExecute == null ? true : canExecute();
    }

    public void Execute(object parameter)
    {
      action();
    }
    #endregion
  }
}
