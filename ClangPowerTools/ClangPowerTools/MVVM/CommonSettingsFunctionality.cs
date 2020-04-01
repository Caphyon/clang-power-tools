using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Models;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ClangPowerTools
{
  public class CommonSettingsFunctionality
  {
    #region Methods

    protected string OpenFile(string fileName, string defaultExt, string filter)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      string path = string.Empty;

      openFileDialog.FileName = fileName;
      openFileDialog.DefaultExt = defaultExt;
      openFileDialog.Filter = filter;

      bool? result = openFileDialog.ShowDialog();

      if (result == true)
      {
        string filename = openFileDialog.FileName;
        path = filename;
      }
      return path;
    }

    protected string SaveFile(string fileName, string defaultExt, string filter)
    {
      SaveFileDialog saveFileDialog = new SaveFileDialog();
      string path = string.Empty;

      // Set the default file extension
      saveFileDialog.FileName = fileName;
      saveFileDialog.DefaultExt = defaultExt;
      saveFileDialog.Filter = filter;

      //Display the dialog window
      bool? result = saveFileDialog.ShowDialog();

      if (result == true)
      {
        path = saveFileDialog.FileName;
      }

      return path;
    }

    protected void WriteContentToFile(string path, string content)
    {
      FileSystem.WriteContentToFile(path, content);
    }

    protected string OpenContentDialog(string content)
    {
      InputDataViewModel inputDataViewModel = new InputDataViewModel(content);
      inputDataViewModel.ShowViewDialog();
      string input = CreateInput(inputDataViewModel.Inputs.ToList());

      return input;
    }

    private string CreateInput(List<InputDataModel> models)
    {
      StringBuilder sb = new StringBuilder();

      foreach (var item in models)
      {
        if (string.IsNullOrWhiteSpace(item.InputData) == false)
          sb.Append(item.InputData).Append(";");
      }

      if (sb.Length > 0)
        sb.Length--;

      return sb.ToString();
    }

    #endregion
  }
}
