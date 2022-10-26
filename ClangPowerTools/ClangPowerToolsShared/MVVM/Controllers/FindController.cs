using ClangPowerTools;
using ClangPowerTools.Views;
using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using ClangPowerToolsShared.MVVM.Models.ToolWindowModels;
using Newtonsoft.Json;
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
    private Dictionary<string, string> pathCommandPairs = new();
    private List<string> commands = new();
    private string pathToClangQuery;
    private string mInteractiveModeDocumentName = string.Empty;

    public List<MenuItem> MenuOptions
    {
      get
      {
        return LookInMenuController.MenuOptions;
      }
    }

    public MenuItem SMenuOption
    {
      get
      {
        return LookInMenuController.MenuOptions.Last();
      }
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

      switch (findToolWindowModel.CurrentViewMatcher.Id)
      {
        case FindCommandIds.kDefaultArgsId:
          {
            commands.Add(MatchConstants.CalledExprDefaultArg.Replace("{0}", findToolWindowModel.DefaultArgsModel
                        .FunctionName).Replace("{1}", findToolWindowModel.DefaultArgsModel.DefaultArgsPosition.ToString()));
            break;
          }
        case FindCommandIds.kCustomMatchesId:
          {
            if ((findToolWindowModel.CustomMatchesModel.Matchers.Length > 0 && findToolWindowModel.CustomMatchesModel.Matchers[0] == 'm'))
            {
              commands.Add(findToolWindowModel.CustomMatchesModel.Matchers);
            }
            else
            {
              commands.Add("m " + findToolWindowModel.CustomMatchesModel.Matchers);
            }
            break;
          }
        default:
          break;
      }
    }

    public void RunPowershellQuery()
    {
      using (StreamWriter sw = File.AppendText(PathConstants.GetPathToFindCommands))
      {
        foreach (var command in commands)
        {
          sw.WriteLine(command);
        }
      }
      DisplayMessageBeforeFind();
      if (LookInMenuController.GetSelectedMenuItem().LookInMenu == LookInMenu.CurrentActiveDocument)
      {
        pathCommandPairs = GetCommandForPowershellInteractiveMode(pathToClangQuery);
        CheckFileNameActiveInteractiveMode(pathCommandPairs.First().Key);
        PowerShellWrapper.InvokeInteractiveMode(pathCommandPairs.First());
      }
      else
      {
        pathCommandPairs = GetCommandForPowershell(pathToClangQuery);
        PowerShellWrapper.InvokePassSequentialCommands(pathCommandPairs);
      }
      DisplayMessageAfterFind();
      File.Delete(PathConstants.GetPathToFindCommands);
    }

    private void DisplayMessageAfterFind()
    {
      CommandControllerInstance.CommandController.DisplayMessage(false, "\nⒾ Find all matches in Error List -> Ⓘ Messages\n");
    }

    private void DisplayMessageBeforeFind()
    {
      CommandControllerInstance.CommandController.DisplayMessage(false, "\n⌛ Please wait ...\n");

      if (!SettingsProvider.CompilerSettingsModel.VerboseMode)
        CommandControllerInstance.CommandController.DisplayMessage(false, "\nYou can activate verbose mode to see the" +
          " complete output. Settings -> Compile -> Verbose mode\n");
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

    private class FileCompilationDB
    {
      public string file { get; set; }
    }

    private Dictionary<string, string> GetCommandForPowershell(string pathToBinary)
    {
      string compilationDatabaseContent = string.Empty;
      if (File.Exists(PathConstants.JsonCompilationDBPath))
      {
        compilationDatabaseContent = File.ReadAllText(PathConstants.JsonCompilationDBPath);
      }
      List<FileCompilationDB> files = JsonConvert.DeserializeObject<List<FileCompilationDB>>(compilationDatabaseContent);

      Dictionary<string, string> commands = new();
      foreach (var path in files.Select(a => a.file).ToList())
      {
        var command = $"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive " +
        $"-command '& ''{pathToBinary}''  ''{path}'' " +
        $"-p ''{PathConstants.JsonCompilationDBPath}'' " +
        $"-f ''{PathConstants.GetPathToFindCommands}'' '";
        if (!commands.ContainsKey(path))
          commands.Add(path, command);
      }
      return commands;
    }

    private void CheckFileNameActiveInteractiveMode(string aFileName)
    {
      if (!string.IsNullOrEmpty(mInteractiveModeDocumentName) && mInteractiveModeDocumentName != aFileName)
        PowerShellWrapper.EndInteractiveMode();
      mInteractiveModeDocumentName = aFileName;
    }

    private Dictionary<string, string> GetCommandForPowershellInteractiveMode(string pathToBinary)
    {
      string compilationDatabaseContent = string.Empty;
      if (File.Exists(PathConstants.JsonCompilationDBPath))
      {
        compilationDatabaseContent = File.ReadAllText(PathConstants.JsonCompilationDBPath);
      }
      List<FileCompilationDB> files = JsonConvert.DeserializeObject<List<FileCompilationDB>>(compilationDatabaseContent);

      Dictionary<string, string> commands = new();
      foreach (var path in files.Select(a => a.file).ToList())
      {
        var command = $"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive " +
        $"-command '& ''{pathToBinary}''  ''{path}'' " +
        $"-p ''{PathConstants.JsonCompilationDBPath}'' '";
        if (!commands.ContainsKey(path))
          commands.Add(path, command);
      }
      return commands;
    }

  }
}
