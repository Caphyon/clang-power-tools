using ClangPowerTools.DialogPages;
using Microsoft.VisualStudio.Shell;
using System;

namespace ClangPowerTools
{
  public static class SettingsProvider
  {

    private static ClangGeneralOptionsView GeneralSettings { get; set; }

    private static ClangTidyOptionsView TidySettings { get; set; }

    private static ClangTidyCustomChecksOptionsView TidyCustomCheckes { get; set; }

    private static ClangTidyPredefinedChecksOptionsView TidyPredefinedChecks { get; set; }

    private static ClangFormatOptionsView ClangFormatSettings { get; set; }


    #region Constructor

    public static void Initialize(Package aPackage)
    {
      GeneralSettings = (ClangGeneralOptionsView)aPackage.GetDialogPage(typeof(ClangGeneralOptionsView));
      TidySettings = (ClangTidyOptionsView)aPackage.GetDialogPage(typeof(ClangTidyOptionsView));
      TidyCustomCheckes = (ClangTidyCustomChecksOptionsView)aPackage.GetDialogPage(typeof(ClangTidyCustomChecksOptionsView));
      TidyPredefinedChecks = (ClangTidyPredefinedChecksOptionsView)aPackage.GetDialogPage(typeof(ClangTidyPredefinedChecksOptionsView));
      ClangFormatSettings = (ClangFormatOptionsView)aPackage.GetDialogPage(typeof(ClangFormatOptionsView));
    }

    #endregion


    #region Public Methods

    public static DialogPage GetSettingsPage(Type aType)
    {
      if (typeof(ClangGeneralOptionsView) == aType)
        return GeneralSettings;

      if (typeof(ClangTidyOptionsView) == aType)
        return TidySettings;

      if (typeof(ClangTidyCustomChecksOptionsView) == aType)
        return TidyCustomCheckes;

      if (typeof(ClangTidyPredefinedChecksOptionsView) == aType)
        return TidyPredefinedChecks;

      if (typeof(ClangFormatOptionsView) == aType)
        return ClangFormatSettings;

      return null;
    }


    #endregion

  }
}
