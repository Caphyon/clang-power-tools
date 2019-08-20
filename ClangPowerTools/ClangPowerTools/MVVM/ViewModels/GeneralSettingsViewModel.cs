using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.Views;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class GeneralSettingsViewModel : INotifyPropertyChanged
  {
    #region Members
    private ICommand logoutCommand;
    private SaveFileDialog saveFileDialog = new SaveFileDialog();

    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    #region Commands
    public ICommand LogoutCommand
    {
      get => logoutCommand ?? (logoutCommand = new RelayCommand(() => LogoutUser(), () => CanExecute));
    }
    #endregion

    #region Properties
    public bool CanExecute
    {
      get
      {
        return true;
      }
    }
    #endregion


    #region Methods
    public void LogoutUser()
    {
      var settingsPathBuilder = new SettingsPathBuilder();
      string path = settingsPathBuilder.GetPath("ctpjwt");

      if (File.Exists(path) == true)
      {
        File.Delete(path);
      }

      LoginView loginView = new LoginView();
      loginView.ShowDialog();

    }

    public void ExportConfig()
    {
      // Set the default file extension
      saveFileDialog.FileName = ".clang-tidy";
      saveFileDialog.DefaultExt = ".clang-tidy";
      saveFileDialog.Filter = "Configuration files (.clang-tidy)|*.clang-tidy";

      //Display the dialog window
      bool? result = saveFileDialog.ShowDialog();

      if (result == true)
      {
        saveFileDialog.FileName = Path.GetFileName(saveFileDialog.FileName);
      }
    }


    private void SaveFileDialog(object sender, CancelEventArgs e)
    {
      CreateFile();
    }

    private void CreateFile()
    {
      using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
      {
        using (StreamWriter sw = new StreamWriter(fs))
        {
          TidyConfigFile tidyConfigFile = new TidyConfigFile();

          sw.Write(tidyConfigFile.CreateOutput());
        }
      }
    }

    #endregion
  }
}
