using ClangPowerTools.Options.View.WpfPropertyGrid;
using ClangPowerTools.Views;
using Microsoft.Win32;
using System.ComponentModel;
using System.Windows.Documents;

namespace ClangPowerTools
{
  public abstract class SettingsViewModel
  {
    #region Members
    protected abstract void ReferenceSettingsHandler();
    private string input { get; set; }
    #endregion;

    #region Methods
    public string BrowseForFile()
    {
      // Create OpenFileDialog
      OpenFileDialog dlg = new OpenFileDialog();
      string path = string.Empty;

      // Set filter for file extension and default file extension
      dlg.DefaultExt = ".exe";
      dlg.Filter = "Executable files|*.exe";

      // Display OpenFileDialog by calling ShowDialog method
      bool? result = dlg.ShowDialog();

      // Get the selected file name and 
      if (result == true)
      {
        // Open document
        string filename = dlg.FileName;
        path = filename;
      }
      return path;
    }


    public string OpenContentDialog(string content)
    {
      AddDataViewModel addDataViewModel = new AddDataViewModel(content);
      addDataViewModel.OpenDialog();
      string input = addDataViewModel.TextBoxInput;

      if (string.IsNullOrEmpty(input))
      {
        return content;
      }

      return input;
    }
    #endregion
  }
}
