using ClangPowerTools;
using ClangPowerToolsShared.MVVM.Provider;
using ClangPowerToolsShared.MVVM.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace ClangPowerToolsShared.MVVM
{
  public class FindToolWindowHandler
  {
    public static Action RefreshHistoryMatchersView;

    private const string matchersHistoryFileName = "matchersHistory.json";
    private string matcherHistoryPath = string.Empty;
    public FindToolWindowHandler()
    {
      var settingsPathBuilder = new SettingsPathBuilder();
      matcherHistoryPath = Path.Combine(settingsPathBuilder.GetPath(matcherHistoryPath),
        matchersHistoryFileName);
    }
    public void Initialize()
    {
      if (File.Exists(matcherHistoryPath))
      {
        LoadFindToolWindowData();
      }
    }


    public void SaveMatchersHistoryData()
    {
      SerializeHistoryData(FindToolWindowProvider.AutoCompleteHistory, matcherHistoryPath);
    }

    private void SerializeHistoryData(List<AutoCompleteHistoryViewModel> models, string path)
    {
      using StreamWriter file = File.CreateText(path);
      var serializer = new JsonSerializer
      {
        Formatting = Formatting.Indented
      };
      serializer.Serialize(file, models);
      file.Close();
    }

    private string ReadFile(string path)
    {
      using StreamReader sw = new StreamReader(path);
      return sw.ReadToEnd();
    }

    public void LoadFindToolWindowData()
    {
      if(File.Exists(matcherHistoryPath))
      {
        string json = ReadFile(matcherHistoryPath);
        DeserializeMatchersHistory(json);
      }
    }

    public void LoadFindToolWindowData(string path)
    {
      if (File.Exists(path))
      {
        string json = ReadFile(path);
        DeserializeMatchersHistory(json);
        RefreshHistoryMatchersView?.Invoke();
      }
    }

    private void DeserializeMatchersHistory(string json)
    {
      try
      {
        var history = JsonConvert.DeserializeObject<List<AutoCompleteHistoryViewModel>>(json);
        FindToolWindowProvider.UpdateAutoCompleteList(history);
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Cannot Load Clang Power Tools Find Tool Window Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

  }
}
