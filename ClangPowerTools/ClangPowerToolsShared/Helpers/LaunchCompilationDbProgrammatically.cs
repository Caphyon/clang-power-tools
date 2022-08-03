using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.MVVM.Constants;

namespace ClangPowerToolsShared.Helpers
{
  public class LaunchCompilationDbProgrammatically
  {
    private string lastHash = string.Empty;
    private MenuItem lastSlectedMenuOption = new();

    public void FromFindToolWindow()
    {
      var currentHash = CryptographyAlgo.HashFile(PathConstants.VcxprojPath);
      if (currentHash == lastHash)
        return;
      if(string.IsNullOrEmpty(lastHash))
      {
        lastHash = currentHash;
      }

      CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kJsonCompilationDatabase,
        CommandUILocation.ViewMenu, null, false);
    }
  }
}
