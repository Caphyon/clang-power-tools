using ClangPowerTools;
using ClangPowerTools.MVVM.Models;
using System.ComponentModel;
using System.Windows.Input;

namespace ClangPowerToolsShared.MVVM.ViewModels
{
  public class AboutSettingsViewModel
  {
    #region Members

    private GeneralSettingsModel generalModel;

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion


    #region Constructor

    public AboutSettingsViewModel()
    {
      generalModel = SettingsProvider.GeneralSettingsModel;
    }

    #endregion


    #region Properties

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

    public string DisplayMessage
    {
      get { return displayMessage; }
      set
      {
        displayMessage = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DisplayMessage"));
      }
    }

    private string displayMessage;

    #endregion
  }
}