using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using System.Threading.Tasks;
using System.Windows.Controls;
using MenuItem = ClangPowerToolsShared.Commands.MenuItem;

namespace ClangPowerToolsShared.Helpers
{
  internal class LaunchCompilationDbProgrammatically
  {
    private string lastHash = string.Empty;
    private MenuItem lastSelectedMenuOption = new();

    public async Task FromFindToolWindowAsync()
    {
      var currentHash = CryptographyAlgo.HashFile(PathConstants.VcxprojPath);

      var selectedItem = LookInMenuController.GetSelectedMenuItem();
      if (lastSelectedMenuOption == selectedItem && lastHash == currentHash &&
        selectedItem.LookInMenu == LookInMenu.EntireSolution)
      {
        return;
      }else if(lastHash != currentHash || string.IsNullOrEmpty(lastHash) ||
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
