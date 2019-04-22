using ClangPowerTools.Helpers;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Tests
{
  public static class UnitTestUtility
  {
    public static string GetVsVersion()
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      var dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
      return dte.Version;
    }

    public static async Task LoadPackageAsync()
    {
      var guid = Guid.Parse(RunClangPowerToolsPackage.PackageGuidString);
      var shell = (IVsShell7)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
      await shell.LoadPackageAsync(ref guid);
    }

    public static string GetPackageVersion()
    {
      return PackageUtility.GetVersion();
    }

    public static bool GetCommandByID(Commands2 command2, string aGuid, int aId, out Command command)
    {
      command = null;

      if (string.IsNullOrWhiteSpace(aGuid))
        return false;

      if (null == command2)
        return false;

      command = command2.Item(aGuid, aId);

      if (null == command)
        return false;

      return true;
    }

    public static bool RunCommand(DTE2 aDte, string aGuid, int aCommandId)
    {
      Commands2 command2 = aDte.Commands as Commands2;

      if (GetCommandByID(command2, aGuid, aCommandId, out Command command))
      {
        aDte.ExecuteCommand(command.Name);
        return true;
      }
      else
      {
        return false;
      }
    }

    public static ClangGeneralOptionsView GetClangGeneralOptionsViewFromFile()
    {
      ClangGeneralOptionsView clangGeneralOptionsView = new ClangGeneralOptionsView();
      clangGeneralOptionsView.LoadSettingsFromStorage();
      return clangGeneralOptionsView;
    }

    public static ClangTidyOptionsView GetClangTidyOptionViewFromFile()
    {
      ClangTidyOptionsView clangTidyOptionsView = new ClangTidyOptionsView();
      clangTidyOptionsView.LoadSettingsFromStorage();
      return clangTidyOptionsView;
    }

    public static ClangTidyCustomChecksOptionsView GetClangTidyCustomChecksViewFromFile()
    {
      ClangTidyCustomChecksOptionsView clangTidyCustomChecksOptionsView = new ClangTidyCustomChecksOptionsView();
      clangTidyCustomChecksOptionsView.LoadSettingsFromStorage();
      return clangTidyCustomChecksOptionsView;
    }

    public static ClangFormatOptionsView GetClangFormatOptionsViewFromFile()
    {
      ClangFormatOptionsView clangFormatOptionsView = new ClangFormatOptionsView();
      clangFormatOptionsView.LoadSettingsFromStorage();
      return clangFormatOptionsView;
    }

    public static void ResetClangGeneralOptionsView()
    {
      SettingsProvider.GeneralSettings.ResetSettings();
    }

    public static void ResetClangFormatOptionsView()
    {
      SettingsProvider.ClangFormatSettings.ResetSettings();
    }

    public static void ResetClangTidyOptionsView()
    {
      SettingsProvider.TidySettings.ResetSettings();
    }

    public static void ResetClangTidyCustomChecksOptionsView()
    {
      SettingsProvider.TidyCustomCheckes.ResetSettings();
    }

    public static void SaveGeneralOptions (ClangGeneralOptionsView clangGeneralOptionsView)
    {
      clangGeneralOptionsView.SaveSettingsToStorage();
    }

    public static void SaveFormatOptions(ClangFormatOptionsView clangFormatOptionsView)
    {
      clangFormatOptionsView.SaveSettingsToStorage();
    }

    public static void SaveTidyOptions(ClangTidyOptionsView clangTidyOptionsView)
    {
      clangTidyOptionsView.SaveSettingsToStorage();
    }

    public static void SaveClangTidyCustomChecksOptionsView(ClangTidyCustomChecksOptionsView clangTidyCustomChecksOptionsView)
    {
      clangTidyCustomChecksOptionsView.SaveSettingsToStorage();
    }
  }
}
