using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClangPowerTools.MVVM.Commands
{
  internal class AddDataCommand : ICommand
  {

    public AddDataCommand()
    {
        
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
      throw new NotImplementedException();
    }

    public void Execute(object parameter)
    {
      throw new NotImplementedException();
    }
  }
}
