using ClangPowerTools.DialogPages;
using Microsoft.VisualStudio.Shell;
using System;

namespace ClangPowerTools
{
  public static class SettingsProvider
  {

    public static ClangGeneralOptionsView GeneralSettings { get; set; }
    public static ClangTidyOptionsView TidySettings { get; set; }
    public static ClangTidyCustomChecksOptionsView TidyCustomCheckes { get; set; }
    public static ClangTidyPredefinedChecksOptionsView TidyPredefinedChecks { get; set; }
    public static ClangFormatOptionsView ClangFormatSettings { get; set; }


    #region Constructor

    public static void Initialize(Package aPackage)
    {
      GeneralSettings = (ClangGeneralOptionsView)aPackage.GetDialogPage(typeof(ClangGeneralOptionsView));
      TidySettings = (ClangTidyOptionsView)aPackage.GetDialogPage(typeof(ClangTidyOptionsView));
      TidyCustomCheckes = (ClangTidyCustomChecksOptionsView)aPackage.GetDialogPage(typeof(ClangTidyCustomChecksOptionsView));
      TidyPredefinedChecks = (ClangTidyPredefinedChecksOptionsView)aPackage.GetDialogPage(typeof(ClangTidyPredefinedChecksOptionsView));
      ClangFormatSettings = (ClangFormatOptionsView)aPackage.GetDialogPage(typeof(ClangFormatOptionsView));
    }

    public static void SaveAll()
    {
      GeneralSettings.SaveSettingsToStorage();
      TidySettings.SaveSettingsToStorage();
      TidyCustomCheckes.SaveSettingsToStorage();
      TidyPredefinedChecks.SaveSettingsToStorage();
      ClangFormatSettings.SaveSettingsToStorage();
    }

    #endregion

  }
}
