using ClangPowerTools.DialogPages;
using Microsoft.VisualStudio.Shell;
using System;

namespace ClangPowerTools
{
  public static class SettingsProvider
  {
    #region Members

    private static Package mPackage;

    #endregion


    #region Constructor

    public static void Initialize(Package aPackage) => mPackage = aPackage;

    #endregion


    #region Public Methods

    public static DialogPage GetSettingsPage(Type aType)
    {
      if (aType == typeof(ClangGeneralOptionsView))
        return mPackage.GetDialogPage(typeof(ClangGeneralOptionsView)) as ClangGeneralOptionsView;

      if (aType == typeof(ClangTidyOptionsView))
        return mPackage.GetDialogPage(typeof(ClangTidyOptionsView)) as ClangTidyOptionsView;

      if (aType == typeof(ClangTidyCustomChecksOptionsView))
        return mPackage.GetDialogPage(typeof(ClangTidyCustomChecksOptionsView)) as ClangTidyCustomChecksOptionsView;

      if (aType == typeof(ClangTidyPredefinedChecksOptionsView))
        return mPackage.GetDialogPage(typeof(ClangTidyPredefinedChecksOptionsView)) as ClangTidyPredefinedChecksOptionsView;

      if (aType == typeof(ClangFormatOptionsView))
        return mPackage.GetDialogPage(typeof(ClangFormatOptionsView)) as ClangFormatOptionsView;

      return null;
    }

    #endregion

  }
}
