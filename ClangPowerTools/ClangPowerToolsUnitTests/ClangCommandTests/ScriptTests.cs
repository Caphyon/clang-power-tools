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

    private static readonly string kCompileProjectScript  = @"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -command '& ''c:\users\enache ionut\appdata\local\microsoft\visualstudio\16.0_ada83f57exp\extensions\caphyon\clang power tools\4.10.3\clang-build.ps1''  -proj ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocatorTest\CustomAllocatorTest.vcxproj'' -active-config ''Debug|x64'' -clang-flags  (''-Wall'',''-fms-compatibility-version=19.10'',''-Wmicrosoft'',''-Wno-invalid-token-paste'',''-Wno-unknown-pragmas'',''-Wno-unused-value'') -parallel -vs-ver 2019 -vs-sku Professional -dir ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocator.sln'''";
    private static readonly string kTidyProjectScript     = @"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -command '& ''c:\users\enache ionut\appdata\local\microsoft\visualstudio\16.0_ada83f57exp\extensions\caphyon\clang power tools\4.10.3\clang-build.ps1''  -proj ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocatorTest\CustomAllocatorTest.vcxproj'' -active-config ''Debug|x64'' -clang-flags  (''-Wall'',''-fms-compatibility-version=19.10'',''-Wmicrosoft'',''-Wno-invalid-token-paste'',''-Wno-unknown-pragmas'',''-Wno-unused-value'') -tidy ''-*,google-readability-braces-around-statements,google-readability-casting,google-readability-function-size,google-readability-namespace-comments,google-readability-todo,modernize-avoid-bind,modernize-avoid-c-arrays,modernize-concat-nested-namespaces,modernize-deprecated-headers,modernize-deprecated-ios-base-aliases,modernize-loop-convert,modernize-make-shared,modernize-make-unique,modernize-pass-by-value,modernize-raw-string-literal,modernize-redundant-void-arg,modernize-replace-auto-ptr,modernize-replace-random-shuffle,modernize-return-braced-init-list,modernize-shrink-to-fit,modernize-unary-static-assert,modernize-use-auto,modernize-use-bool-literals,modernize-use-default-member-init,modernize-use-emplace,modernize-use-equals-default,modernize-use-equals-delete,modernize-use-noexcept,modernize-use-nullptr,modernize-use-override,modernize-use-transparent-functors,modernize-use-uncaught-exceptions,modernize-use-using,readability-avoid-const-params-in-decls,readability-braces-around-statements,readability-const-return-type,readability-container-size-empty,readability-deleted-default,readability-delete-null-pointer,readability-else-after-return,readability-function-size,readability-identifier-naming,readability-implicit-bool-conversion,readability-inconsistent-declaration-parameter-name,readability-isolate-declaration,readability-magic-numbers,readability-misleading-indentation,readability-misplaced-array-index,readability-named-parameter,readability-non-const-parameter,readability-redundant-control-flow,readability-redundant-declaration,readability-redundant-function-ptr-dereference,readability-redundant-member-init,readability-redundant-smartptr-get,readability-redundant-string-cstr,readability-redundant-string-init,readability-simplify-boolean-expr,readability-simplify-subscript-expr,readability-static-accessed-through-instance,readability-static-definition-in-anonymous-namespace,readability-string-compare,readability-uniqueptr-delete-release,readability-uppercase-literal-suffix'' -header-filter ''.*'' -vs-ver 2019 -vs-sku Professional -dir ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocator.sln'''";
    private static readonly string kTidyFixProjectScript  = @"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -command '& ''c:\users\enache ionut\appdata\local\microsoft\visualstudio\16.0_ada83f57exp\extensions\caphyon\clang power tools\4.10.3\clang-build.ps1''  -proj ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocatorTest\CustomAllocatorTest.vcxproj'' -active-config ''Debug|x64'' -clang-flags  (''-Wall'',''-fms-compatibility-version=19.10'',''-Wmicrosoft'',''-Wno-invalid-token-paste'',''-Wno-unknown-pragmas'',''-Wno-unused-value'') -tidy-fix ''-*,google-readability-braces-around-statements,google-readability-casting,google-readability-function-size,google-readability-namespace-comments,google-readability-todo,modernize-avoid-bind,modernize-avoid-c-arrays,modernize-concat-nested-namespaces,modernize-deprecated-headers,modernize-deprecated-ios-base-aliases,modernize-loop-convert,modernize-make-shared,modernize-make-unique,modernize-pass-by-value,modernize-raw-string-literal,modernize-redundant-void-arg,modernize-replace-auto-ptr,modernize-replace-random-shuffle,modernize-return-braced-init-list,modernize-shrink-to-fit,modernize-unary-static-assert,modernize-use-auto,modernize-use-bool-literals,modernize-use-default-member-init,modernize-use-emplace,modernize-use-equals-default,modernize-use-equals-delete,modernize-use-noexcept,modernize-use-nullptr,modernize-use-override,modernize-use-transparent-functors,modernize-use-uncaught-exceptions,modernize-use-using,readability-avoid-const-params-in-decls,readability-braces-around-statements,readability-const-return-type,readability-container-size-empty,readability-deleted-default,readability-delete-null-pointer,readability-else-after-return,readability-function-size,readability-identifier-naming,readability-implicit-bool-conversion,readability-inconsistent-declaration-parameter-name,readability-isolate-declaration,readability-magic-numbers,readability-misleading-indentation,readability-misplaced-array-index,readability-named-parameter,readability-non-const-parameter,readability-redundant-control-flow,readability-redundant-declaration,readability-redundant-function-ptr-dereference,readability-redundant-member-init,readability-redundant-smartptr-get,readability-redundant-string-cstr,readability-redundant-string-init,readability-simplify-boolean-expr,readability-simplify-subscript-expr,readability-static-accessed-through-instance,readability-static-definition-in-anonymous-namespace,readability-string-compare,readability-uniqueptr-delete-release,readability-uppercase-literal-suffix'' -header-filter ''.*'' -vs-ver 2019 -vs-sku Professional -dir ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocator.sln'''";

    #endregion

    #region Test Methods

    [VsFact(Version = "2017-")]
    public async System.Threading.Tasks.Task CreateProjectCompileScript_UIAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      //Act
      LoadSolution();
      SettingsTestUtility.ResetClangGeneralOptionsView();
      await CompileCommand.Instance.RunClangCompileAsync(CommandIds.kCompileId, CommandUILocation.Toolbar);
      CloseSolution();

      //Assert
      Assert.Equal(CompileCommand.Instance.Script, kCompileProjectScript);
    }


    [VsFact(Version = "2017-")]
    public async System.Threading.Tasks.Task CreateProjectTidyScript_UIAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      SettingsTestUtility.ResetAllSettings();
      LoadSolution();
      await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyId, CommandUILocation.Toolbar);
      CloseSolution();

      Assert.Equal(TidyCommand.Instance.Script, kTidyProjectScript);
    }


    [VsFact(Version = "2017-")]
    public async System.Threading.Tasks.Task CreateProjectTidyFixScript_UIAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();

      SettingsTestUtility.ResetAllSettings();
      LoadSolution();
      await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyFixId, CommandUILocation.Toolbar);
      CloseSolution();

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

    private void CloseSolution()
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      var dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
      Assumes.Present(dte);

      dte.Solution.Close();
    }

    #endregion

  }
}
