using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using System.Threading.Tasks;
using MenuItem = ClangPowerToolsShared.Commands.MenuItem;

namespace ClangPowerToolsShared.Helpers
{
  internal class LaunchCompilationDbProgrammatically
  {
    private string lastHash = string.Empty;
    private MenuItem lastSelectedMenuOption = new();

    /// <summary>
    /// Before launching compilation database programmatically, you need to check selected option from menu
    /// </summary>
    /// <returns></returns>
    public async Task FromFindToolWindowAsync()
    {
      var currentHash = CryptographyAlgo.HashFile(PathConstants.VcxprojPath);

      //Generate again compilation database on project, document, or files modifications
      var selectedItem = LookInMenuController.GetSelectedMenuItem();
      if (lastSelectedMenuOption == selectedItem && lastHash == currentHash &&
        selectedItem.LookInMenu == LookInMenu.EntireSolution)
      {
        return;
      }
      else if (lastHash != currentHash || string.IsNullOrEmpty(lastHash) ||
        lastSelectedMenuOption != LookInMenuController.GetSelectedMenuItem())
      {
        lastHash = currentHash;
        lastSelectedMenuOption = LookInMenuController.GetSelectedMenuItem();
      }
      await CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kJsonCompilationDatabase,
        CommandUILocation.ViewMenu, null, false);
    }

    public async Task FromGenerateDocumentationAsync()
    {
      await CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kJsonCompilationDatabase, CommandUILocation.ContextMenu,
             null, false);
    }
  }
}
