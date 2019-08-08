using Microsoft.Win32;

namespace ClangPowerTools
{
  public abstract class CommonSettingsFunctionality
  {
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
      InputDataViewModel inputDataViewModel = new InputDataViewModel(content);
      inputDataViewModel.ShowViewDialog();
      string input = inputDataViewModel.TextBoxInput;

      if (string.IsNullOrEmpty(input))
      {
        return content;
      }
      return input;
    }
    #endregion
  }
}
