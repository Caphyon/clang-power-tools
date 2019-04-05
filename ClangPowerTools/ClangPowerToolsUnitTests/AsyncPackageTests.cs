using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Tests
{
  public class AsyncPackageTests
  {
    [VsTheory(Version = "2019")]
    [InlineData(RunClangPowerToolsPackage.PackageGuidString, true)]
    [InlineData("11111111-2222-3333-4444-555555555555", false)]
    public async Task LoadTestAsync(string guidString, bool expectedSuccess)
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      IVsShell7 shell = (IVsShell7)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
      await UnitTestUtility.LoadPackageAsync();
      VsServiceProvider.TryGetService(typeof(DTE), out object dteService);

      //Act
      var dte = dteService as DTE;
      Commands2 commands = dte.Commands as Commands2;
      Assumes.Present(shell);
      var guid = Guid.Parse(guidString);

      //Assert
      if (expectedSuccess)
        await shell.LoadPackageAsync(ref guid);
      else
        await Assert.ThrowsAnyAsync<Exception>(async () => await shell.LoadPackageAsync(ref guid));
    }
  }
}
