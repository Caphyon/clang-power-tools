using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Xunit;
using ClangPowerTools;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace ClangPowerTools.Tests
{
  [VsTestSettings(UIThread = true)]
  public class VsServiceProviderTests
  {
    [VsTheory(Version = "2019")]
    [InlineData()]
    public async Task DteServiceWasRegistered_TestAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      var guid = Guid.Parse(RunClangPowerToolsPackage.PackageGuidString);
      var shell = (IVsShell7)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
      await shell.LoadPackageAsync(ref guid);

      VsServiceProvider.TryGetService(typeof(DTE), out object dteService);

      DTE2 dte = dteService as DTE2;

      Command command = dte.Commands.Item("498fdff5-5217-4da9-88d2-edad44ba3874", 0x0102);
 
      string test = command.LocalizedName;
      var test1 = command.ID;
      var test2 = command.Guid;
      var test3 = command.ToString();

      Assert.NotNull(dteService as DTE);
    }


    [VsTheory(Version = "2019")]
    [InlineData()]
    public async Task OutputWindowServiceWasRegistered_TestAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      var guid = Guid.Parse(RunClangPowerToolsPackage.PackageGuidString);
      var shell = (IVsShell7)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
      await shell.LoadPackageAsync(ref guid);

      VsServiceProvider.TryGetService(typeof(SVsOutputWindow), out object outputWinfowService);
      Assert.NotNull(outputWinfowService as IVsOutputWindow);
    }

  }
}
