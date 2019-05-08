using ClangPowerTools.Commands;
using ClangPowerTools.Tests;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Xunit;
using ClangPowerTools;

namespace ClangPowerToolsUnitTests.ClangCommandTests
{

  [VsTestSettings(UIThread = true)]
  public class ScriptTests
  {
    #region Members

    private string kSolutionPath = @"C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocator.sln";

    private static readonly string kCompileProjectScript = @"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -command '& ''c:\users\enache ionut\appdata\local\microsoft\visualstudio\16.0_ada83f57exp\extensions\caphyon\clang power tools\4.10.3\clang-build.ps1''  -proj ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocatorTest\CustomAllocatorTest.vcxproj'' -active-config ''Debug|x64'' -clang-flags  (''-Wall'',''-fms-compatibility-version=19.10'',''-Wmicrosoft'',''-Wno-invalid-token-paste'',''-Wno-unknown-pragmas'',''-Wno-unused-value'') -file-ignore (''DispatcherHandler.cpp'',''VsServiceProviderTests.cpp'',''AsyncPackageTests.cpp'') -parallel -vs-ver 2019 -vs-sku Professional -dir ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocator.sln'''";
    private static readonly string kTidyProjectScript = @"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -command '& ''c:\users\enache ionut\appdata\local\microsoft\visualstudio\16.0_ada83f57exp\extensions\caphyon\clang power tools\4.10.3\clang-build.ps1''  -proj ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocatorTest\CustomAllocatorTest.vcxproj'' -active-config ''Debug|x64'' -clang-flags  (''-Wall'',''-fms-compatibility-version=19.10'',''-Wmicrosoft'',''-Wno-invalid-token-paste'',''-Wno-unknown-pragmas'',''-Wno-unused-value'') -file-ignore (''DispatcherHandler.cpp'',''VsServiceProviderTests.cpp'',''AsyncPackageTests.cpp'') -tidy ''.clang-tidy'' -header-filter ''.*'' -vs-ver 2019 -vs-sku Professional -dir ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocator.sln'''";
    private static readonly string kTidyFixProjectScript = @"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -command '& ''c:\users\enache ionut\appdata\local\microsoft\visualstudio\16.0_ada83f57exp\extensions\caphyon\clang power tools\4.10.3\clang-build.ps1''  -proj ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocatorTest\CustomAllocatorTest.vcxproj'' -active-config ''Debug|x64'' -clang-flags  (''-Wall'',''-fms-compatibility-version=19.10'',''-Wmicrosoft'',''-Wno-invalid-token-paste'',''-Wno-unknown-pragmas'',''-Wno-unused-value'') -file-ignore (''DispatcherHandler.cpp'',''VsServiceProviderTests.cpp'',''AsyncPackageTests.cpp'') -tidy-fix ''.clang-tidy'' -header-filter ''.*'' -vs-ver 2019 -vs-sku Professional -dir ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocator.sln'''";

    #endregion

    #region Test Methods

    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task CreateProjectCompileScript_UIAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      //Act
      LoadSolution();
      await CompileCommand.Instance.RunClangCompileAsync(CommandIds.kCompileId, CommandUILocation.Toolbar);

      //Assert
      Assert.Equal(CompileCommand.Instance.Script, kCompileProjectScript);
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task CreateProjectTidyScript_UIAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      LoadSolution();
      await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyId, CommandUILocation.Toolbar);

      Assert.Equal(TidyCommand.Instance.Script, kTidyProjectScript);
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task CreateProjectTidyFixScript_UIAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      LoadSolution();
      await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyFixId, CommandUILocation.Toolbar);

      Assert.Equal(TidyCommand.Instance.Script, kTidyFixProjectScript);
    }


    #endregion


    #region Private Methods


    private void LoadSolution()
    {
      ThreadHelper.ThrowIfNotOnUIThread();

      var dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
      Assumes.Present(dte);

      dte.Solution.Open(kSolutionPath);

      var build = dte.Solution.SolutionBuild;
      build.Build(true);
    }

    #endregion

  }
}
