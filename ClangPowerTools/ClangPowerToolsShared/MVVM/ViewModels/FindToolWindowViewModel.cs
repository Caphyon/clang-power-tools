using ClangPowerTools;
using ClangPowerTools.Views;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using ClangPowerToolsShared.MVVM.Models.ToolWindowModels;
using ClangPowerTools.MVVM.Command;
using System.Threading.Tasks;
using ClangPowerTools.Commands;
using ClangPowerToolsShared.MVVM.Controllers;
using System.Collections.ObjectModel;
using ClangPowerToolsShared.Commands;

namespace ClangPowerToolsShared.MVVM.ViewModels
{

  public class FindToolWindowViewModel : FindController
  {
    private FindToolWindowView findToolWindowView;
    private List<string> filesPaths = new();

    public List<KeyValuePair<int, string>> Matchers
    {
      get { return FindCommandIds.Matchers; }
    }

    public FindToolWindowViewModel(FindToolWindowView findToolWindowView)
    {
      this.findToolWindowView = findToolWindowView;
      findToolWindowModel.DefaultArgsModel.Show();
    }

    public void OpenToolWindow(List<string> filesPath)
    {
      SetCommandId(FindCommandIds.kDefaultArgsId);
      filesPaths = filesPath;
    }

    public void RunQuery()
    {
      SelectCommandToRun(currentCommandId);
      RunPowershellQuery();
      AfterCommand();
    }

    public void SelectCommandToRun(int commandId)
    {
      SetCommandId(commandId);
      LaunchCommand(currentCommandId, filesPaths, FindToolWindowModel);
    }

    public void RunCommandFromView()
    {
      BeforeCommand();
      CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kClangFindRun, CommandUILocation.ContextMenu);

    }
  } 
}
