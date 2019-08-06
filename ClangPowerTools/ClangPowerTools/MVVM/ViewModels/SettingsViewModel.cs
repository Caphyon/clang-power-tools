using ClangPowerTools.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class SettingsViewModel
  {
    #region Members
    private CPTSettings cptSettings = new CPTSettings();
    private SettingsView settingsView;

    #endregion;

    #region Constructors
    public SettingsViewModel()
    {
      settingsView = new SettingsView(this);
    }
    #endregion

    #region Methods
    public void ShowViewDialog()
    {
      cptSettings.DeserializeSettings();
      settingsView.ShowDialog();
    }

    public void CloseViewDialog()
    {
      cptSettings.SerializeSettings();
    }
    #endregion
  }
}
