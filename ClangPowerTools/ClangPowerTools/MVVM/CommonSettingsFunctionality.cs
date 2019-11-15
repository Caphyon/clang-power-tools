using Microsoft.Win32;
using System.IO;

namespace ClangPowerTools
{
  public class CommonSettingsFunctionality
  {
    #region Public Methods
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

    protected string OpenContentDialog(string content)
    {
      InputDataViewModel inputDataViewModel = new InputDataViewModel(content);
      inputDataViewModel.ShowViewDialog();
      string input = string.Join(";",  inputDataViewModel.Inputs);
      //TODO check last ;

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
