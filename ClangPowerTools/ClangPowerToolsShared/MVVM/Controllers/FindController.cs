using ClangPowerTools;
using ClangPowerTools.Helpers;
using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using ClangPowerToolsShared.MVVM.Models.ToolWindowModels;
using System.Collections.Generic;
using System.IO;

namespace ClangPowerToolsShared.MVVM.Controllers
{
  public class FindController
  {
    private int currentCommand;
    private string pathToClangQuery;
    public FindToolWindowModel FindToolWindowModel = new();

    public FindController()
    {
      currentCommand = 0;
      pathToClangQuery = string.Empty;
    }

    public void LaunchCommand(int commandId, List<string> paths)
    {
      currentCommand = commandId;
      GetPathToClangQuery();

      switch (currentCommand)
      {
        case FindCommandIds.kDefaultArgs:
        {
          List<string> commands = new();
            commands.Add(GetListPowershell(paths, pathToClangQuery));
            //commands.Add(MatchConstants.CalledExprDefaultArg.Replace("{0}", ))
          break;
        }
        default:
          break;
      }
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
