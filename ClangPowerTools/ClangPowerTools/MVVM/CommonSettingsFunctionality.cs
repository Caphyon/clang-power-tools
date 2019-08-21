using Microsoft.Win32;
using System.IO;

namespace ClangPowerTools
{
  public abstract class CommonSettingsFunctionality
  {
    #region Public Methods
    protected string BrowseForFile(string defaultExt, string filter)
    {
      OpenFileDialog dlg = new OpenFileDialog();
      string path = string.Empty;

      dlg.DefaultExt = defaultExt;
      dlg.Filter = filter;

      bool? result = dlg.ShowDialog();

      if (result == true)
      {
        string filename = dlg.FileName;
        path = filename;
      }
      return path;
    }

    protected string OpenContentDialog(string content)
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
      using (FileStream fs = new FileStream(path, FileMode.Create))
      {
        using (StreamWriter sw = new StreamWriter(fs))
        {
          sw.Write(content);
        }
      }
    }
    #endregion
  }
}
