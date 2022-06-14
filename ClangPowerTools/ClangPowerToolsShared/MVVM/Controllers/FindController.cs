using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.Helpers;
using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using ClangPowerToolsShared.MVVM.Models.ToolWindowModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace ClangPowerToolsShared.MVVM.Controllers
{
  public class FindController : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    protected int currentCommandId;
    private string pathToClangQuery;
    string script = string.Empty;
    protected FindToolWindowModel findToolWindowModel = new();
    List<string> commands = new();

    public FindToolWindowModel FindToolWindowModel
    {
      get { return findToolWindowModel; }
      set
      {
        findToolWindowModel = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FindToolWindowModel"));
      }
    }
    public FindController()
    {
      currentCommandId = 0;
      pathToClangQuery = string.Empty;
    }

    public void LaunchCommand(int commandId, List<string> paths, FindToolWindowModel findToolWindowModel)
    {
      SetCommandId(commandId);
      if (pathToClangQuery == string.Empty)
        GetPathToClangQuery();
      script = GetListPowershell(paths, pathToClangQuery);

      if (commands.Count > 0)
        commands.Clear();

      commands.Add(MatchConstants.SetOutpuDump);

      switch (currentCommandId)
      {
        case FindCommandIds.kDefaultArgsId:
          {
            findToolWindowModel.DefaultArgsModel.Show();
            commands.Add(MatchConstants.CalledExprDefaultArg.Replace("{0}", findToolWindowModel.DefaultArgsModel
                        .FunctionName).Replace("{1}", findToolWindowModel.DefaultArgsModel.DefaultArgsPosition.ToString()));
            break;
          }
        default:
          break;
      }
    }

    public void RunPowershellQuery()
    {
      using (StreamWriter sw = File.AppendText(PathConstants.GetPathToFindCommands()))
      {
        foreach (var command in commands)
        {
          sw.WriteLine(command);
        }
      }
      CommandControllerInstance.CommandController.DisplayMessage(false, "\n⌛ Please wait ...\n");
      PowerShellWrapper.InvokePassSequentialCommands(script);
      CommandControllerInstance.CommandController.DisplayMessage(false, "\nⒾ Find all matches in Error List -> Ⓘ Messages\n");
      File.Delete(PathConstants.GetPathToFindCommands());
    }

    protected void BeforeCommand()
    {
      findToolWindowModel.IsRunning = true;
      FindToolWindowModel = findToolWindowModel;
    }

    protected void AfterCommand()
    {
      findToolWindowModel.IsRunning = false;
      FindToolWindowModel = findToolWindowModel;
    }

    private void GetPathToClangQuery()
    {
      if (pathToClangQuery == string.Empty)
      {
        pathToClangQuery = PowerShellWrapper.DownloadTool(ScriptConstants.kQueryFile);
        pathToClangQuery = Path.Combine(pathToClangQuery, ScriptConstants.kQueryFile);
      }
    }

    protected void SetCommandId(int commandId)
    {
      currentCommandId = commandId;
    }

    private string GetListPowershell(List<string> args, string pathToBinary)
    {
      var paths = args.Where(a => ScriptConstants.kAcceptedFileExtensionsWithoutHeaders
      .Contains(Path.GetExtension(a))).ToList();
      return $"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive " +
        $"-command '& ''{pathToBinary}''  @{ScriptGenerator.JoinPathsToStringScript(paths)} " +
        $"-p ''{JsonCompilationDatabaseCommand.Instance.JsonDBPath}'' " +
        $"-f ''{PathConstants.GetPathToFindCommands()}'' '";
    }

  }
}
