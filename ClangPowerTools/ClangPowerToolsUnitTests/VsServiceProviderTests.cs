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
    public async Task DteService_SuccessfulRegistrationAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      VsServiceProvider.TryGetService(typeof(DTE), out object dteService);

      //Assert
      Assert.NotNull(dteService as DTE);
    }

    [VsFact(Version = "2019")]
    public async Task Dte2Service_SuccessfulRegistrationAsync()
    {
      // Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      VsServiceProvider.TryGetService(typeof(DTE), out object dteService);

      // Assert
      Assert.NotNull(dteService as DTE2);
    }

    [VsFact(Version = "2019")]
    public async Task OutputWindowService_SuccessfulRegistrationAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      VsServiceProvider.TryGetService(typeof(SVsOutputWindow), out object outputWindowService);

      // Assert
      Assert.NotNull(outputWindowService as IVsOutputWindow);
    }

    // ----------------------------------------------------------------------------
    [VsFact(Version = "2019")]
    public async Task VsStatusbarService_SuccessfulRegistrationAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      VsServiceProvider.TryGetService(typeof(SVsStatusbar), out object statusBarService);

      // Assert
      Assert.NotNull(statusBarService as IVsStatusbar);
    }

    [VsFact(Version = "2019")]
    public async Task RunningDocumentTableService_SuccessfulRegistrationAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      VsServiceProvider.TryGetService(typeof(SVsRunningDocumentTable), out object runningDocumentTableService);

      // Assert
      Assert.NotNull(runningDocumentTableService as IVsRunningDocumentTable);
    }

    [VsFact(Version = "2019")]
    public async Task FileChangeService_SuccessfulRegistrationAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      VsServiceProvider.TryGetService(typeof(SVsFileChangeEx), out object fileChangeService);

      // Assert
      Assert.NotNull(fileChangeService as IVsFileChangeEx);
    }

    [VsFact(Version = "2019")]
    public async Task SolutionService_SuccessfulRegistrationAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      VsServiceProvider.TryGetService(typeof(SVsSolution), out object vsSolutionService);

      // Assert
      Assert.NotNull(vsSolutionService as IVsSolution);
    }

  }
}
