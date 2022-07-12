using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.Views;
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
    protected FindToolWindowView findToolWindowView;
    protected FindToolWindowModel findToolWindowModel = new();
    Dictionary<string, string> pathCommandPairs = new();
    List<string> commands = new();
    private string pathToClangQuery;

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
      pathToClangQuery = string.Empty;
    }

    public void LaunchCommand()
    {
      if (pathToClangQuery == string.Empty)
        GetPathToClangQuery();

      if (commands.Count > 0)
        commands.Clear();

      commands.Add(MatchConstants.SetOutpuDump);

      switch (findToolWindowModel.CurrentCommandId)
      {
        case FindCommandIds.kDefaultArgsId:
          {
            commands.Add(MatchConstants.CalledExprDefaultArg.Replace("{0}", findToolWindowModel.DefaultArgsModel
                        .FunctionName).Replace("{1}", findToolWindowModel.DefaultArgsModel.DefaultArgsPosition.ToString()));
            break;
          }
        case FindCommandIds.kCustomMatchesId:
          {
            commands.Add(findToolWindowModel.CustomMatchesModel.Matches);
            break;
          }
        default:
          break;
      }
      FindToolWindowModel = findToolWindowModel;
    }

    public void RunPowershellQuery(List<string> paths)
    {
      using (StreamWriter sw = File.AppendText(PathConstants.GetPathToFindCommands()))
      {
        foreach (var command in commands)
        {
          sw.WriteLine(command);
        }
      }
      CommandControllerInstance.CommandController.DisplayMessage(false, "\n⌛ Please wait ...\n");
      pathCommandPairs = GetCommandForPowershell(paths, pathToClangQuery);
      PowerShellWrapper.InvokePassSequentialCommands(pathCommandPairs);
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

    private Dictionary<string, string> GetCommandForPowershell(List<string> args, string pathToBinary)
    {
      var paths = args.Where(a => ScriptConstants.kAcceptedFileExtensionsWithoutHeaders
                      .Contains(Path.GetExtension(a))).ToList();

      Dictionary<string, string> commands = new();
      foreach (var path in paths)
      {
        var command = $"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive " +
        $"-command '& ''{pathToBinary}''  ''{path}'' " +
        $"-p ''{JsonCompilationDatabaseCommand.Instance.JsonDBPath}'' " +
        $"-f ''{PathConstants.GetPathToFindCommands()}'' '";
        if (!commands.ContainsKey(path))
          commands.Add(path, command);
      }
      return commands;
    }

  }
}
