using ClangPowerTools.MVVM.Commands;
using System.ComponentModel;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class LlvmSettingsViewModel
  {
    #region Members

    private ICommand dowloadCommand;
    private ICommand deleteCommand;
    private ICommand stopCommand;

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

    #region Commands
    public ICommand DownloadCommand
    {
      get => dowloadCommand ?? (dowloadCommand = new RelayCommand(() => DownloadLlvmVersion(), () => CanExecute));
    }

    public ICommand DeleteCommand
    {
      get => deleteCommand ?? (deleteCommand = new RelayCommand(() => DeleteLlvmVersion(), () => CanExecute));
    }

    public ICommand StopCommand
    {
      get => stopCommand ?? (stopCommand = new RelayCommand(() => StopDownload(), () => CanExecute));
    }

    #endregion

    #region Methods
    private void DownloadLlvmVersion()
    {

    }

    private void DeleteLlvmVersion()
    {

    }

    private void StopDownload()
    {

    }
    #endregion
  }
}
