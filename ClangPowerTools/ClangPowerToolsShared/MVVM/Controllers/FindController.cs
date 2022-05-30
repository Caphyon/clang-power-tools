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
    string script = string.Empty;
    List<string> commands = new();
    public FindController()
    {
      currentCommand = 0;
      pathToClangQuery = string.Empty;
    }

    public void LaunchCommand(int commandId, List<string> paths, FindToolWindowModel findToolWindowModel)
    {
      currentCommand = commandId;
      if(pathToClangQuery == string.Empty)
        GetPathToClangQuery();
      script = GetListPowershell(paths, pathToClangQuery);

      if(commands.Count > 0)
        commands.Clear();
      
      commands.Add(MatchConstants.SetOutpuDump);

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
      }
    }

    public void RunQuery()
    {
      PowerShellWrapper.InvokePassSequentialCommands(commands, script);
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
