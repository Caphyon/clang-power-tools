using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class LoginViewModel : INotifyPropertyChanged
  {
    #region Members

    private UserModel userModel = new UserModel();
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    #region Properties

    public string Email
    {
      get { return userModel.email; }
      set
      {
        userModel.email = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Email"));
      }
    }

    public string Password
    {
      get { return userModel.password; }
      set
      {
        userModel.password = value;
      }
    }
    #endregion

  }
}
