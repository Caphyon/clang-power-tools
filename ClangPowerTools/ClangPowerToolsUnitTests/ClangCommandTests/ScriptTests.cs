using ClangPowerTools.Builder;
using ClangPowerTools.Commands;
using ClangPowerTools.Helpers;
using ClangPowerTools.Script;
using ClangPowerTools.Services;
using ClangPowerTools.Views;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ClangPowerTools.Tests.ClangCommandTests
{

  [VsTestSettings(UIThread = true)]
  public class ScriptTests
  {
    #region Members

    private readonly string kSolutionPath = @"C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocator.sln";

    private static readonly string kCompileProjectScript = @"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -command '& ''c:\users\enache ionut\appdata\local\microsoft\visualstudio\16.0_ada83f57exp\extensions\caphyon\clang power tools\4.10.3\clang-build.ps1''  -proj ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocatorTest\CustomAllocatorTest.vcxproj'' -active-config ''Debug|x64'' -clang-flags  (''-Wall'',''-fms-compatibility-version=19.10'',''-Wmicrosoft'',''-Wno-invalid-token-paste'',''-Wno-unknown-pragmas'',''-Wno-unused-value'') -parallel -vs-ver 2019 -vs-sku Professional -dir ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocator.sln'''";
    private static readonly string kTidyProjectScript = @"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -command '& ''c:\users\enache ionut\appdata\local\microsoft\visualstudio\16.0_ada83f57exp\extensions\caphyon\clang power tools\4.10.3\clang-build.ps1''  -proj ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocatorTest\CustomAllocatorTest.vcxproj'' -active-config ''Debug|x64'' -clang-flags  (''-Wall'',''-fms-compatibility-version=19.10'',''-Wmicrosoft'',''-Wno-invalid-token-paste'',''-Wno-unknown-pragmas'',''-Wno-unused-value'') -tidy ''-*,google-readability-braces-around-statements,google-readability-casting,google-readability-function-size,google-readability-namespace-comments,google-readability-todo,modernize-avoid-bind,modernize-avoid-c-arrays,modernize-concat-nested-namespaces,modernize-deprecated-headers,modernize-deprecated-ios-base-aliases,modernize-loop-convert,modernize-make-shared,modernize-make-unique,modernize-pass-by-value,modernize-raw-string-literal,modernize-redundant-void-arg,modernize-replace-auto-ptr,modernize-replace-random-shuffle,modernize-return-braced-init-list,modernize-shrink-to-fit,modernize-unary-static-assert,modernize-use-auto,modernize-use-bool-literals,modernize-use-default-member-init,modernize-use-emplace,modernize-use-equals-default,modernize-use-equals-delete,modernize-use-noexcept,modernize-use-nullptr,modernize-use-override,modernize-use-transparent-functors,modernize-use-uncaught-exceptions,modernize-use-using,readability-avoid-const-params-in-decls,readability-braces-around-statements,readability-const-return-type,readability-container-size-empty,readability-deleted-default,readability-delete-null-pointer,readability-else-after-return,readability-function-size,readability-identifier-naming,readability-implicit-bool-conversion,readability-inconsistent-declaration-parameter-name,readability-isolate-declaration,readability-magic-numbers,readability-misleading-indentation,readability-misplaced-array-index,readability-named-parameter,readability-non-const-parameter,readability-redundant-control-flow,readability-redundant-declaration,readability-redundant-function-ptr-dereference,readability-redundant-member-init,readability-redundant-smartptr-get,readability-redundant-string-cstr,readability-redundant-string-init,readability-simplify-boolean-expr,readability-simplify-subscript-expr,readability-static-accessed-through-instance,readability-static-definition-in-anonymous-namespace,readability-string-compare,readability-uniqueptr-delete-release,readability-uppercase-literal-suffix'' -header-filter ''.*'' -vs-ver 2019 -vs-sku Professional -dir ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocator.sln'''";
    private static readonly string kTidyFixProjectScript = @"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -command '& ''c:\users\enache ionut\appdata\local\microsoft\visualstudio\16.0_ada83f57exp\extensions\caphyon\clang power tools\4.10.3\clang-build.ps1''  -proj ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocatorTest\CustomAllocatorTest.vcxproj'' -active-config ''Debug|x64'' -clang-flags  (''-Wall'',''-fms-compatibility-version=19.10'',''-Wmicrosoft'',''-Wno-invalid-token-paste'',''-Wno-unknown-pragmas'',''-Wno-unused-value'') -tidy-fix ''-*,google-readability-braces-around-statements,google-readability-casting,google-readability-function-size,google-readability-namespace-comments,google-readability-todo,modernize-avoid-bind,modernize-avoid-c-arrays,modernize-concat-nested-namespaces,modernize-deprecated-headers,modernize-deprecated-ios-base-aliases,modernize-loop-convert,modernize-make-shared,modernize-make-unique,modernize-pass-by-value,modernize-raw-string-literal,modernize-redundant-void-arg,modernize-replace-auto-ptr,modernize-replace-random-shuffle,modernize-return-braced-init-list,modernize-shrink-to-fit,modernize-unary-static-assert,modernize-use-auto,modernize-use-bool-literals,modernize-use-default-member-init,modernize-use-emplace,modernize-use-equals-default,modernize-use-equals-delete,modernize-use-noexcept,modernize-use-nullptr,modernize-use-override,modernize-use-transparent-functors,modernize-use-uncaught-exceptions,modernize-use-using,readability-avoid-const-params-in-decls,readability-braces-around-statements,readability-const-return-type,readability-container-size-empty,readability-deleted-default,readability-delete-null-pointer,readability-else-after-return,readability-function-size,readability-identifier-naming,readability-implicit-bool-conversion,readability-inconsistent-declaration-parameter-name,readability-isolate-declaration,readability-magic-numbers,readability-misleading-indentation,readability-misplaced-array-index,readability-named-parameter,readability-non-const-parameter,readability-redundant-control-flow,readability-redundant-declaration,readability-redundant-function-ptr-dereference,readability-redundant-member-init,readability-redundant-smartptr-get,readability-redundant-string-cstr,readability-redundant-string-init,readability-simplify-boolean-expr,readability-simplify-subscript-expr,readability-static-accessed-through-instance,readability-static-definition-in-anonymous-namespace,readability-string-compare,readability-uniqueptr-delete-release,readability-uppercase-literal-suffix'' -header-filter ''.*'' -vs-ver 2019 -vs-sku Professional -dir ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocator.sln'''";

    private readonly string compileFileResult = @"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -command '& ''c:\users\enache ionut\appdata\local\microsoft\visualstudio\16.0_ada83f57exp\extensions\caphyon\clang power tools\5.2.1\clang-build.ps1''  -proj ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocatorTest\CustomAllocatorTest.vcxproj'' -file ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocatorTest\CustomAllocatorTest.cpp'' -active-config ''Debug|x64'' -clang-flags  (''-Wall'',''-fms-compatibility-version=19.10'',''-Wmicrosoft'',''-Wno-invalid-token-paste'',''-Wno-unknown-pragmas'',''-Wno-unused-value'') -parallel -vs-ver 2019 -vs-sku Professional -dir ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocator.sln'' '";
    private readonly string tidyFileResult = @"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -command '& ''c:\users\enache ionut\appdata\local\microsoft\visualstudio\16.0_ada83f57exp\extensions\caphyon\clang power tools\5.2.1\clang-build.ps1''  -proj ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocatorTest\CustomAllocatorTest.vcxproj'' -file ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocatorTest\CustomAllocatorTest.cpp'' -active-config ''Debug|x64'' -clang-flags  (''-Wall'',''-fms-compatibility-version=19.10'',''-Wmicrosoft'',''-Wno-invalid-token-paste'',''-Wno-unknown-pragmas'',''-Wno-unused-value'') -tidy ''-*,modernize-avoid-bind,modernize-avoid-c-arrays,modernize-concat-nested-namespaces,modernize-deprecated-headers,modernize-deprecated-ios-base-aliases,modernize-loop-convert,modernize-make-shared,modernize-make-unique,modernize-pass-by-value,modernize-raw-string-literal,modernize-redundant-void-arg,modernize-replace-auto-ptr,modernize-replace-random-shuffle,modernize-return-braced-init-list,modernize-shrink-to-fit,modernize-unary-static-assert,modernize-use-auto,modernize-use-bool-literals,modernize-use-default-member-init,modernize-use-emplace,modernize-use-equals-default,modernize-use-equals-delete,modernize-use-nodiscard,modernize-use-noexcept,modernize-use-nullptr,modernize-use-override,modernize-use-trailing-return-type,modernize-use-transparent-functors,modernize-use-uncaught-exceptions,modernize-use-using,readability-avoid-const-params-in-decls,readability-braces-around-statements,readability-const-return-type,readability-container-size-empty,readability-convert-member-functions-to-static,readability-deleted-default,readability-delete-null-pointer,readability-else-after-return,readability-identifier-naming,readability-implicit-bool-conversion,readability-inconsistent-declaration-parameter-name,readability-isolate-declaration,readability-magic-numbers,readability-misleading-indentation,readability-misplaced-array-index,readability-named-parameter,readability-non-const-parameter,readability-redundant-control-flow,readability-redundant-declaration,readability-redundant-function-ptr-dereference,readability-redundant-member-init,readability-redundant-preprocessor,readability-redundant-smartptr-get,readability-redundant-string-cstr,readability-redundant-string-init,readability-simplify-boolean-expr,readability-simplify-subscript-expr,readability-static-accessed-through-instance,readability-static-definition-in-anonymous-namespace,readability-string-compare,readability-uniqueptr-delete-release,readability-uppercase-literal-suffix'' -header-filter ''.*'' -vs-ver 2019 -vs-sku Professional -dir ''C:\GitRepos\ClangPowerToolsTests\CustomAllocator\CustomAllocator.sln'''";

    private readonly Dictionary<string, string> mVsVersions = new Dictionary<string, string>
    {
      {"11.0", "2010"},
      {"12.0", "2012"},
      {"13.0", "2013"},
      {"14.0", "2015"},
      {"15.0", "2017"},
      {"16.0", "2019"}
    };

    #endregion

    #region Test Methods

    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task CreateProjectItemCompileScript_UIAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      var settingsHandler = new SettingsHandler();

      //Act
      settingsHandler.ResetSettings();
      LoadSolution();

      var dte = (DTE2)VsServiceProvider.GetService(typeof(DTE));
      mVsVersions.TryGetValue(dte.Version, out string version);

      var projectItem = dte.Solution.Projects.Item(1).ProjectItems.Item(4);
      var currentProjectItem = new CurrentProjectItem(projectItem);

      var runModeParameters = ScriptGenerator.GetRunModeParamaters();
      var genericParameters = ScriptGenerator.GetGenericParamaters(CommandIds.kCompileId, dte.Edition, version);
      var itemRelatedParameters = ScriptGenerator.GetItemRelatedParameters(currentProjectItem);

      var compileFileTest = JoinUtility.Join(" ", runModeParameters.Remove(runModeParameters.Length - 1), itemRelatedParameters, genericParameters, "'");

      CloseSolution();

      //Assert
      Assert.Equal(compileFileTest, compileFileResult);
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task CreateProjectItemTidyScript_UIAsync()
    {
      //Arrange
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      var settingsHandler = new SettingsHandler();

      //Act
      settingsHandler.ResetSettings();
      LoadSolution();

      var tidyChecksView = new TidyChecksView();
      SettingsProvider.TidyChecksViewModel.TidyChecksView = tidyChecksView;

      var dte = (DTE2)VsServiceProvider.GetService(typeof(DTE));
      mVsVersions.TryGetValue(dte.Version, out string version);

      var projectItem = dte.Solution.Projects.Item(1).ProjectItems.Item(4);
      var currentProjectItem = new CurrentProjectItem(projectItem);

      var runModeParameters = ScriptGenerator.GetRunModeParamaters();
      var genericParameters = ScriptGenerator.GetGenericParamaters(CommandIds.kTidyId, dte.Edition, version);
      var itemRelatedParameters = ScriptGenerator.GetItemRelatedParameters(currentProjectItem);

      var tidyFileTest = JoinUtility.Join(" ", runModeParameters.Remove(runModeParameters.Length - 1), itemRelatedParameters, genericParameters, "'");

      CloseSolution();

      //Assert
      Assert.Equal(tidyFileTest, tidyFileResult);
    }


    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task CreateProjectTidyFixScript_UIAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      await UnitTestUtility.LoadPackageAsync();
      var settingsHandler = new SettingsHandler();

      settingsHandler.ResetSettings();
      LoadSolution();
      await CommandTestUtility.CommandController.LaunchCommandAsync(CommandIds.kTidyFixId, CommandUILocation.ContextMenu);
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
