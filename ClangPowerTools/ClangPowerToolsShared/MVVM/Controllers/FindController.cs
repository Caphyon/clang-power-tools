using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.Helpers;
using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using ClangPowerToolsShared.MVVM.Models.ToolWindowModels;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ClangPowerToolsShared.MVVM.Controllers
{
  public class FindController
  {
    private int currentCommand;
    private string pathToClangQuery;
    List<string> commands = new();

    public FindController()
    {
      currentCommand = 0;
      pathToClangQuery = string.Empty;
    }

    public void LaunchCommand(int commandId, List<string> paths, FindToolWindowModel findToolWindowModel)
    {
      currentCommand = commandId;
      GetPathToClangQuery();
      if (commands.Count > 0)
      {
        commands.Clear();
        commands.Add(GetListPowershell(paths, pathToClangQuery));
      }else
      {
        commands.Add(GetListPowershell(paths, pathToClangQuery));
      }

      switch (currentCommand)
      {
        case FindCommandIds.kDefaultArgs:
          {

            commands.Add(MatchConstants.CalledExprDefaultArg.Replace("{0}", findToolWindowModel.DefaultArgs
                        .FunctionName).Replace("{1}", findToolWindowModel.DefaultArgs.DefaultArgsPosition.ToString()));
            break;
          }
        default:
          break;

         CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kClangFindRun, CommandUILocation.ContextMenu);

      }
    }

    public void RunQuery()
    {
      PowerShellWrapper.InvokePassSequentialCommands(commands);
    }

    private void GetPathToClangQuery()
    {
      if (pathToClangQuery == string.Empty)
      {
        pathToClangQuery = PowerShellWrapper.DownloadTool(ScriptConstants.kQueryFile);
        pathToClangQuery = Path.Combine(pathToClangQuery, ScriptConstants.kQueryFile);
      }
    }

    private string GetListPowershell(List<string> args, string pathToBinary)
    {
      return $"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive " +
        $"-command '& ''{pathToBinary}'' @{ScriptGenerator.JoinPathsToStringScript(args)} '";
    }

  }
}
