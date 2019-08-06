using Microsoft.Win32;
using System.ComponentModel;
using System.Windows.Documents;

namespace ClangPowerTools
{
  public abstract class SettingsViewModel
  {
    protected abstract void ReferenceSettingsHandler();

    public string BrowseFile()
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

  }
}
