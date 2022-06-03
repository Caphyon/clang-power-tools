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

  public class FindToolWindowViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private FindToolWindowView findToolWindowView;
    private FindToolWindowModel findToolWindowModel = new();
    private FindController findController = new();
    private List<string> filesPaths = new();
    private int currentCommandId = 0;

    public List<KeyValuePair<int, string>> Matchers
    {
      get { return FindCommandIds.Matchers; }
    }

    public FindToolWindowViewModel(FindToolWindowView findToolWindowView)
    {
      this.findToolWindowView = findToolWindowView;
    }

    public FindToolWindowModel FindToolWindowModel
    {
      get { return findToolWindowModel; }
      set
      {
        findToolWindowModel = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FindToolWindowModel"));
      }
    }

    public void OpenToolWindow(List<string> filesPath)
    {
      filesPaths = filesPath;
    }

    public void RunQuery()
    {
      findController.RunQuery();
    }

    public void SelectCommandToRun(int commandId)
    {
      currentCommandId = commandId;
      findController.LaunchCommand(currentCommandId, filesPaths, FindToolWindowModel);
      CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kClangFindRun, CommandUILocation.ContextMenu);
    }
  }
}
