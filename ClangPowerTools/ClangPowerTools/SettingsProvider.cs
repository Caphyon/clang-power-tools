using ClangPowerTools.DialogPages;
using Microsoft.VisualStudio.Shell;
using System;

namespace ClangPowerTools
{
  public static class SettingsProvider
  {
    #region Properties

    public static ClangGeneralOptionsView GeneralSettings { get; private set; }

    public static ClangTidyOptionsView TidySettings { get; private set; }

    public static ClangTidyCustomChecksOptionsView TidyCustomChecks { get; private set; }

    public static ClangTidyPredefinedChecksOptionsView TidyPredefinedChecks { get; private set; }

    public static ClangFormatOptionsView ClangFormatSettings { get; private set; }

    #endregion


    #region Constructor

    public static void Initialize(Package aPackage)
    {
      GeneralSettings = aPackage.GetDialogPage(typeof(ClangGeneralOptionsView)) as ClangGeneralOptionsView;
      TidySettings = aPackage.GetDialogPage(typeof(ClangTidyOptionsView)) as ClangTidyOptionsView;
      TidyCustomChecks = aPackage.GetDialogPage(typeof(ClangTidyCustomChecksOptionsView)) as ClangTidyCustomChecksOptionsView;
      TidyPredefinedChecks = aPackage.GetDialogPage(typeof(ClangTidyPredefinedChecksOptionsView)) as ClangTidyPredefinedChecksOptionsView;
      ClangFormatSettings = aPackage.GetDialogPage(typeof(ClangFormatOptionsView)) as ClangFormatOptionsView;
    }

    #endregion


    #region Public Methods

    public static DialogPage GetSettingsPage(Type aType)
    {
      if (aType == typeof(ClangGeneralOptionsView))
        return GeneralSettings;

      if (aType == typeof(ClangTidyOptionsView))
        return TidySettings;

      if (aType == typeof(ClangTidyCustomChecksOptionsView))
        return TidyCustomChecks;

      if (aType == typeof(ClangTidyPredefinedChecksOptionsView))
        return TidyPredefinedChecks;

      if (aType == typeof(ClangFormatOptionsView))
        return ClangFormatSettings;

      return null;
    }

    #endregion


  }
}
