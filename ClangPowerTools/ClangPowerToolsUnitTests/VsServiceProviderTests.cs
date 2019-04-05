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
    [VsFact(Version = "2019")]
    public async Task DteServiceWasRegisteredAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      VsServiceProvider.TryGetService(typeof(DTE), out object dteService);

      //Assert
      Assert.NotNull(dteService as DTE);
    }


    [VsFact(Version = "2019")]
    public async Task Dte2ServiceWasRegisteredAsync()
    {
      // Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      VsServiceProvider.TryGetService(typeof(DTE), out object dteService);

      // Assert
      Assert.NotNull(dteService as DTE2);
    }


    [VsFact(Version = "2019")]
    public async Task OutputWindowServiceWasRegisteredAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      VsServiceProvider.TryGetService(typeof(SVsOutputWindow), out object outputWindowService);

      // Assert
      Assert.NotNull(outputWindowService as IVsOutputWindow);
    }

  }
}
