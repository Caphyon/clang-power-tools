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
    private string mActiveDocumentName = string.Empty;

    /// <summary>
    /// Before launching compilation database programmatically, you need to check selected option from menu
    /// </summary>
    /// <returns></returns>
    public async Task FromFindToolWindowAsync()
    {
      var currentHash = PathConstants.VcxprojPath.GetHashCode().ToString();

      var selectedItem = LookInMenuController.GetSelectedMenuItem();

      bool sameActiveDocument = true;
      if (DocumentHandler.GetActiveDocument()?.FullName != mActiveDocumentName
        && selectedItem.LookInMenu == LookInMenu.CurrentActiveDocument)
      {
        sameActiveDocument = false;
        mActiveDocumentName = DocumentHandler.GetActiveDocument()?.FullName;
      }

      //Generate again compilation database on project, document, or files modifications
      if ((lastSelectedMenuOption == selectedItem && lastHash == currentHash &&
        selectedItem.LookInMenu == LookInMenu.EntireSolution) || (selectedItem.LookInMenu == LookInMenu.CurrentActiveDocument && sameActiveDocument))
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
