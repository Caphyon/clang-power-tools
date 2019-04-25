using ClangPowerTools.Tests;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ClangPowerToolsUnitTests.ClangCommandTests
{

  [VsTestSettings(UIThread = true)]
  public class ClangCompileTests
  {
    private string kSolutionPath = @"D:\Clang Power Tools Stuff\TestProjects\Github\CustomAlocator-master\CustomAllocator.sln";
    private string kFilePath = @"D:\Clang Power Tools Stuff\TestProjects\Github\CustomAlocator-master\CustomAllocatorTest\CustomAllocatorTest.cpp";

    private static readonly string kOpenDocumentCommandName = "File.OpenFile";

    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task Script_UIAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      var dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
      Assumes.Present(dte);

      dte.Solution.Open(kSolutionPath);

      dte.ExecuteCommand(kOpenDocumentCommandName, kFilePath);

      System.Threading.Thread.Sleep(5000);

      Assert.True(true);
    }


  }
}
