using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.LicenseValidation;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.MVVM.Views;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class AccountViewModel : INotifyPropertyChanged
  {
    #region Members

    private string accoutCellHeight;
    private AccountModel accountModel;
    private GeneralSettingsModel generalModel;

    private ICommand logoutCommand;

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion


    #region Constructor

    public AccountViewModel()
    {
      accountModel = SettingsProvider.AccountModel;
      generalModel = SettingsProvider.GeneralSettingsModel;
    }

    #endregion


    #region Properties

    public AccountModel AccountModel
    {
      get
      {
        SetAccountNameToTrial();
        return accountModel;
      }
      set
      {
        accountModel = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AccountModel"));
      }
    }

    public string AccoutCellHeight
    {
      get
      {
        return accoutCellHeight;
      }
      set
      {
        accoutCellHeight = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AccoutCellHeight"));
      }
    }

    public string Alignment
    {
      get
      {
        return accoutCellHeight;
      }
      set
      {
        accoutCellHeight = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AccoutCellHeight"));
      }
    }

    public GeneralSettingsModel GeneralSettingsModel
    {
      get
      {
        return generalModel;
      }
      set
      {
        generalModel = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("GeneralSettingsModel"));
      }
    }
    public string DisplayMessage { get { return displayMessage; }
      set 
      { 
        displayMessage = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DisplayMessage"));
      } 
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    public bool DisplayLogout { get; set; }

    public bool DisplayLogIn { get; set; }

    public bool Visible { get; set; }

    public bool DisplayUserNameAndEmail { get; set; }
    private string displayMessage;

    #endregion


    #region Commands

    public ICommand LogoutCommand
    {
      get => logoutCommand ??= new RelayCommand(() => Logout(), () => CanExecute);
    }

    #endregion


    #region Methods

    private void Logout()
    {
      var settingsPathBuilder = new SettingsPathBuilder();
      string path = settingsPathBuilder.GetPath("ctpjwt");

      if (File.Exists(path))
        File.Delete(path);

      SettingsProvider.AccountModel = new AccountModel();
      SettingsProvider.SettingsView.Close();

      LoginView loginView = new LoginView();
      loginView.ShowDialog();
    }


    private void SetAccountNameToTrial()
    {
      if (accountModel.LicenseType == LicenseType.Commercial || accountModel.LicenseType == LicenseType.Personal)
      {
        AccoutCellHeight = "auto";
        DisplayUserNameAndEmail = true;
        Visible = accountModel.LicenseType == LicenseType.Commercial;
        DisplayLogIn = false;
        DisplayLogout = true;
        Alignment = "Left";
        displayMessage = Visibility.Hidden.ToString();

      }
      else
      {
        displayMessage = Visibility.Visible.ToString();
        AccoutCellHeight = "0";
        accountModel.UserName = string.Empty;
        DisplayUserNameAndEmail = false;
        Visible = false;
        DisplayLogIn = true;
        DisplayLogout = false;
        Alignment = "Center";
      }
      DisplayMessage = displayMessage;
    }

    #endregion
  }
}
