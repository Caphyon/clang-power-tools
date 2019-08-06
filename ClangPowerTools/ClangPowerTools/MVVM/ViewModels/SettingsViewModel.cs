using Microsoft.Win32;

namespace ClangPowerTools
{
  public abstract class SettingsViewModel
  {
    #region Abstract Methods
    protected abstract void ReferenceSettingsHandler();
    #endregion;

    #region Methods
    public string BrowseForFile()
    {
      OpenFileDialog dlg = new OpenFileDialog();
      string path = string.Empty;

      dlg.DefaultExt = ".exe";
      dlg.Filter = "Executable files|*.exe";

      bool? result = dlg.ShowDialog();

      if (result == true)
      {
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
