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
    async Task LoadTestAsync(string guidString, bool expectedSuccess)
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      var shell = (IVsShell7)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
      var dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));

      var commands = dte.Commands as Commands2;

      Command cmd;
      if (null != commands)
        cmd = commands.Item("498fdff5-5217-4da9-88d2-edad44ba3874", 0x0102);

      Assumes.Present(shell);

      var guid = Guid.Parse(guidString);

      if (expectedSuccess)
        await shell.LoadPackageAsync(ref guid);
      else
        await Assert.ThrowsAnyAsync<Exception>(async () => await shell.LoadPackageAsync(ref guid));
    }
  }
}
