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
    public async Task DteServiceWasRegistered_Test()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      var guid = Guid.Parse(RunClangPowerToolsPackage.PackageGuidString);
      var shell = (IVsShell7)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
      await shell.LoadPackageAsync(ref guid);

      VsServiceProvider.TryGetService(typeof(DTE), out object dteService);
      Assert.NotNull(dteService as DTE);
    }


    [VsTheory(Version = "2019")]
    [InlineData()]
    public async Task OutputWindowServiceWasRegistered_Test()
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
