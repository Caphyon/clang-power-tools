﻿using ClangPowerTools;
using ClangPowerTools.Views;
using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using ClangPowerToolsShared.MVVM.Models.ToolWindowModels;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Shapes;

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
    private int mInteractiveModeDocumentHash = 0;
    private string activeDocumentPaths = string.Empty;
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

    public string ActiveDocumentPaths
    {
      get { return activeDocumentPaths; }
      set
      {
        activeDocumentPaths = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ActiveDocumentPaths"));
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
      if (!string.IsNullOrEmpty(ActiveDocumentPaths))
      {
        CommandControllerInstance.CommandController.DisplayMessage(false, $"\nⒾ We will query paths: {ActiveDocumentPaths} \n");
        pathCommandPairs = GetCommandForPowershellInteractiveMode(pathToClangQuery, ActiveDocumentPaths);
        CheckFileNameActiveInteractiveMode(pathCommandPairs.First().Key);
        PowerShellWrapper.InvokeInteractiveMode(pathCommandPairs.First());
      } else if (LookInMenuController.GetSelectedMenuItem().LookInMenu == LookInMenu.CurrentActiveDocument)
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
      CommandControllerInstance.CommandController.DisplayMessage(false, "\nⒾ Find all matches in Error List -> Ⓘ Messages \nMake sure that [Build + IntelliSense] is selected\n");
    }

    private void DisplayMessageBeforeFind()
    {
      CommandControllerInstance.CommandController.DisplayMessage(false, "\n⌛ Please wait ...\n");

      // 1 - verbose
      if (SettingsProvider.CompilerSettingsModel.VerbosityLevel != "1")
        CommandControllerInstance.CommandController.DisplayMessage(false, "\nYou can select verbose mode to see the" +
          " complete output. Settings -> Compile -> Verbosity level\n");
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
        pathToClangQuery = System.IO.Path.Combine(pathToClangQuery, ScriptConstants.kQueryFile);
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
        var command = $"-ExecutionPolicy Unrestricted -NoProfile -Noninteractive " +
        $"-command \" & '{pathToBinary}'  '{path}' " +
        $"-p '{PathConstants.JsonCompilationDBPath}' " +
        $"-f '{PathConstants.GetPathToFindCommands}' \"";
        if (!commands.ContainsKey(path))
          commands.Add(path, command);
      }
      return commands;
    }

    private void CheckFileNameActiveInteractiveMode(string aFileName)
    {
      bool modifedFile = false;
      if (File.Exists(aFileName))
      {
        int contentHash = File.ReadAllText(aFileName).GetHashCode();
        if (contentHash != mInteractiveModeDocumentHash)
          modifedFile = true;
        mInteractiveModeDocumentHash = contentHash;
      }

      if (!string.IsNullOrEmpty(mInteractiveModeDocumentName)
        && (mInteractiveModeDocumentName != aFileName || modifedFile))
        PowerShellWrapper.EndInteractiveMode();
      mInteractiveModeDocumentName = aFileName;
    }

    private Dictionary<string, string> GetCommandForPowershellInteractiveMode(string pathToBinary, string paths = "")
    {
      string compilationDatabaseContent = string.Empty;
      if (File.Exists(PathConstants.JsonCompilationDBPath))
      {
        compilationDatabaseContent = File.ReadAllText(PathConstants.JsonCompilationDBPath);
      }
      List<FileCompilationDB> files = JsonConvert.DeserializeObject<List<FileCompilationDB>>(compilationDatabaseContent);

      Dictionary<string, string> commands = new();
      if (!string.IsNullOrEmpty(paths))
      {

        var command = $"-ExecutionPolicy Unrestricted -NoProfile -Noninteractive " +
        $"-command \"& '{pathToBinary}'  @({paths}) " +
        $"-p '{PathConstants.JsonCompilationDBPath}' \"";
        if (!commands.ContainsKey(paths))
          commands.Add(paths, command);

        return commands;
      }

      foreach (var path in files.Select(a => a.file).ToList())
      {
        var command = $"-ExecutionPolicy Unrestricted -NoProfile -Noninteractive " +
        $"-command \"& '{pathToBinary}'  '{path}' " +
        $"-p '{PathConstants.JsonCompilationDBPath}' \"";
        if (!commands.ContainsKey(path))
          commands.Add(path, command);
      }
      return commands;
    }

  }
}
