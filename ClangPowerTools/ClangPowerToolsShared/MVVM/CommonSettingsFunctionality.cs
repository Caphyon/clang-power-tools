using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Models;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ClangPowerTools
{
  public abstract class CommonSettingsFunctionality
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

    protected string[] OpenFiles(string fileName, string defaultExt, string filter)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();

      openFileDialog.FileName = fileName;
      openFileDialog.DefaultExt = defaultExt;
      openFileDialog.Filter = filter;
      openFileDialog.Multiselect = true;

      if (openFileDialog.ShowDialog() != true)
        return null;

      return openFileDialog.FileNames;
    }


    /// <summary>
    /// Browse for folder from where the files path acording to the given seach instruction will be collected 
    /// </summary>
    /// <param name="searchFilePattern">Search pattern to apply in the file search</param>
    /// <param name="searchOption">Information about how to search inside the selected folder</param>
    /// <returns>Array of files path</returns>
    protected string[] BrowseForFolderFiles(string searchFilePattern, SearchOption searchOption)
    {
      using var folderBrowseDialog = new System.Windows.Forms.FolderBrowserDialog();
      System.Windows.Forms.DialogResult result = folderBrowseDialog.ShowDialog();

      if (result != System.Windows.Forms.DialogResult.OK || string.IsNullOrWhiteSpace(folderBrowseDialog.SelectedPath))
        return null;

      return Directory.GetFiles(folderBrowseDialog.SelectedPath, searchFilePattern, searchOption);
    }


    /// <summary>
    /// Browse for folder path 
    /// </summary>
    /// <param name="searchFilePattern">Search pattern to apply in the file search</param>
    /// <param name="searchOption">Information about how to search inside the selected folder</param>
    /// <returns>Path to the selected folder</returns>
    protected string BrowseForFolderFiles()
    {
      using var folderBrowseDialog = new System.Windows.Forms.FolderBrowserDialog();
      System.Windows.Forms.DialogResult result = folderBrowseDialog.ShowDialog();

      if (result != System.Windows.Forms.DialogResult.OK || string.IsNullOrWhiteSpace(folderBrowseDialog.SelectedPath))
        return null;

      return folderBrowseDialog.SelectedPath;
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

    protected string OpenContentDialog(string content, bool showFilesPicker = false, bool showFolderPicker = false)
    {
      InputDataViewModel inputDataViewModel = new InputDataViewModel(content, showFilesPicker, showFolderPicker);
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
