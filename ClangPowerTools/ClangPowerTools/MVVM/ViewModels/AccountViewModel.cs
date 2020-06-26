using ClangPowerTools.MVVM.Models;
using System.ComponentModel;

namespace ClangPowerTools
{
  public class AccountViewModel : INotifyPropertyChanged
  {
    #region Members

    private AccountModel accountModel;
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion


    #region Constructor

    public AccountViewModel()
    {
      accountModel = SettingsProvider.AccountModel;
    }

    #endregion


    #region Properties

    public AccountModel AccountModel
    {
      get
      {
        return accountModel;
      }
      set
      {
        accountModel = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AccountModel"));
      }
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    #endregion


    #region Methods




    #endregion
  }
}
