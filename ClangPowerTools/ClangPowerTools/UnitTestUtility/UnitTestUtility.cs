using ClangPowerTools.Commands;
using ClangPowerTools.Helpers;
using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft;
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

    public static bool RunCommand(DTE2 aDte, string aGuid)
    {
      Commands2 command2 = aDte.Commands as Commands2;

      if (GetCommandByID(command2, aGuid, CommandIds.kSettingsId, out Command command))
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
  }
}
