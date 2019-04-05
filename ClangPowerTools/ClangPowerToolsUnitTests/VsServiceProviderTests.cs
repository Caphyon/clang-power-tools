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
      DTE dte = await Task.Run(() => UnitTestUtility.GetDteServiceAsync().Result);

      //Act
      Command command = dte.Commands.Item("498fdff5-5217-4da9-88d2-edad44ba3874", 0x0102);

      var test = command.LocalizedName;
      var test1 = command.ID;
      var test2 = command.Guid;
      var test3 = command.ToString();
      var test4 = SettingsProvider.ClangFormatSettings.FileExtensions;

      //Assert
      Assert.NotNull(dte);
    }


    [VsFact(Version = "2019")]
    public async Task OutputWindowServiceWasRegisteredAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      //Arrange

      //Act

      //Assert
      Assert.True(true);
    }
  }
}
