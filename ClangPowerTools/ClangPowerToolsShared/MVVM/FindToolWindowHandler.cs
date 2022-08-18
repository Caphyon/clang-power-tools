using ClangPowerTools;
using ClangPowerToolsShared.MVVM.Provider;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace ClangPowerToolsShared.MVVM
{
  public class FindToolWindowHandler
  {
    public static Action RefreshSettingsView;

    private const string matchersHistoryFileName = "matchersHistory.json";
    private string matcherHistoryPath = string.Empty;
    public FindToolWindowHandler()
    {
      var settingsPathBuilder = new SettingsPathBuilder();
      matcherHistoryPath = settingsPathBuilder.GetPath(matcherHistoryPath);
    }
    public void InitializeSettings()
    {
      if (File.Exists(matcherHistoryPath))
      {
        LoadFindToolWindowData();
      }
      else
      {
        CreateDeaultFindToolWindow();
      }
    }

    private void CreateDeaultFindToolWindow()
    {
      FindToolWindowProvider.AutoCompleteHistory = new List<string>();
    }

    public void SaveMatchersHiistoryData()
    {
      List<string> matchersHiistoryData = new List<string>();
      matchersHiistoryData = FindToolWindowProvider.AutoCompleteHistory;
      SerializeHistoryData(matchersHiistoryData);
    }

    private void SerializeHistoryData(List<string> matchersHiistoryData)
    {
      FileInfo fileInfo;
      if (File.Exists(matcherHistoryPath))
      {
        fileInfo = new FileInfo(matcherHistoryPath);
        fileInfo.Attributes &= ~FileAttributes.Hidden;

        // Overwrite the file
        using StreamWriter file = new StreamWriter(matcherHistoryPath);
        var serializer = new JsonSerializer
        {
          Formatting = Formatting.Indented
        };
        serializer.Serialize(file, matchersHiistoryData);

        // Set back the hidden attribute
        fileInfo = new FileInfo(matcherHistoryPath);
        fileInfo.Attributes |= FileAttributes.Hidden;
      }
    }

    private void LoadFindToolWindowData()
    {
      if(File.Exists(matcherHistoryPath))
      {
        string json = ReadFile(matcherHistoryPath);
        DeserializeMatchersHistory(json);
      }
    }

    private string ReadFile(string path)
    {
      using StreamReader sw = new StreamReader(path);
      return sw.ReadToEnd();
    }

    private void LoadFindToolWindowData(string path)
    {
      if (File.Exists(path))
      {
        string json = ReadFile(path);
        DeserializeMatchersHistory(json);
        RefreshSettingsView?.Invoke();
      }
    }

    private void DeserializeMatchersHistory(string json)
    {
      try
      {
        var history = JsonConvert.DeserializeObject<List<string>>(json);
        FindToolWindowProvider.AutoCompleteHistory = history;
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Cannot Load Clang Power Tools Find Tool Window Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

  }
}
