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
      else
      {
        CreateDeaultFindToolWindow();
      }
    }

    private void CreateDeaultFindToolWindow()
    {
      FindToolWindowProvider.AutoCompleteHistory = new List<AutoCompleteHistoryViewModel>();
    }

    public void SaveMatchersHiistoryData()
    {
      FindToolWindowProvider.AutoCompleteHistory.Add(new AutoCompleteHistoryViewModel
      { Name = "test", Value = "a test matcher", RememberAsFavorit = true });
      List<object> models = new List<object>
      {
        FindToolWindowProvider.AutoCompleteHistory
      };

      SerializeHistoryData(models, matcherHistoryPath);
    }

    private void SerializeHistoryData(List<AutoCompleteHistoryViewModel> matchersHiistoryData)
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

    private void SerializeHistoryData(List<object> models, string path)
    {
      using StreamWriter file = File.CreateText(path);
      var serializer = new JsonSerializer
      {
        Formatting = Formatting.Indented
      };
      serializer.Serialize(file, models);
    }

    //private void SerializeSettings(object models, string path)
    //{
    //  // Remove the hidden attribute of the file in order to overwrite it
    //  FileInfo fileInfo;
    //  if (File.Exists(path))
    //  {
    //    fileInfo = new FileInfo(path);
    //    fileInfo.Attributes &= ~FileAttributes.Hidden;
    //  }

    //  // Overwrite the file
    //  using StreamWriter file = new StreamWriter(path);
    //  var serializer = new JsonSerializer
    //  {
    //    Formatting = Formatting.Indented
    //  };
    //  serializer.Serialize(file, models);

    //  // Set back the hidden attribute
    //  fileInfo = new FileInfo(path);
    //  fileInfo.Attributes |= FileAttributes.Hidden;
    //}


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
        FindToolWindowProvider.AutoCompleteHistory = history;
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Cannot Load Clang Power Tools Find Tool Window Data", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

  }
}
