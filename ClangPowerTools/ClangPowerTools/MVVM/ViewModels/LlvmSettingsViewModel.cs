using ClangPowerTools.Handlers;
using ClangPowerTools.MVVM.Controllers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;

namespace ClangPowerTools
{
  public class LlvmSettingsViewModel : INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;
    public CancelEventHandler WindowClosed;

    private readonly LlvmController llvmController = new LlvmController();
    private readonly SettingsProvider settingsProvider = new SettingsProvider();
    private CompilerSettingsModel compilerModel = new CompilerSettingsModel();
    private List<LlvmSettingsModel> llvms = new List<LlvmSettingsModel>();
    private const string uninstall = "Uninstall";

    #endregion

    #region Constructor

    public LlvmSettingsViewModel()
    {
      llvmController.InstallFinished = InstallFinished;
      llvmController.UninstallFinished = UninstallFinished;
      llvmController.onOperationCanceldEvent += OperationCanceled;
      WindowClosed += llvmController.SettingsWindowClosed;
      IntitializeView();
    }
    #endregion

    #region Properties

    public List<LlvmSettingsModel> Llvms
    {
      get
      {
        return llvms;
      }

      set
      {
        llvms = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Llvms"));
      }
    }

    public ObservableCollection<string> InstalledLlvms { get; set; } = new ObservableCollection<string>();

    public string VersionUsed
    {
      get
      {
        return compilerModel.LlvmVersion;
      }

      set
      {
        compilerModel.LlvmVersion = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VersionUsed"));
      }
    }

    #endregion

    #region Public Methods

    public void DownloadCommand(int elementIndex)
    {
      DisableButtons(elementIndex);
      llvmController.llvmModel = llvms[elementIndex];
      llvmController.llvmModel.IsDownloading = true;
      llvmController.Download(llvmController.llvmModel.Version, DownloadProgressChanged);
    }

    public void CancelCommand()
    {
      ResetButtonsState();
      llvmController.llvmModel.DownloadProgress = 0;
      llvmController.llvmModel.IsDownloading = false;
      llvmController.downloadCancellationToken.Cancel();
    }

    public void UninstallCommand(int elementIndex)
    {
      DisableButtons(elementIndex);
      llvmController.llvmModel = llvms[elementIndex];
      llvmController.Uninstall(llvmController.llvmModel.Version);
    }

    #endregion

    #region Private Methods

    private void InstallFinished(object sender, EventArgs e)
    {
      ResetButtonsState();
      UIUpdater.InvokeAsync(InsertVersionToInstalledLlvms).SafeFireAndForget();
    }


    private void UninstallFinished(object sender, EventArgs e)
    {
      ResetVersionUsedIfRequired();
      ResetButtonsState();
      UIUpdater.InvokeAsync(new Action(() =>
      {
        InstalledLlvms.Remove(llvmController.llvmModel.Version);
      })).SafeFireAndForget();
    }

    private void OperationCanceled()
    {
      ResetButtonsState();
    }

    private void IntitializeView()
    {
      foreach (var version in LlvmVersions.Versions)
      {
        var llvmModel = new LlvmSettingsModel()
        {
          Version = version,
          IsInstalled = llvmController.IsVersionExeOnDisk(version, uninstall),
        };

        if (llvmModel.IsInstalled)
        {
          InstalledLlvms.Add(llvmModel.Version);
        }

        llvms.Add(llvmModel);
      }

      compilerModel = settingsProvider.GetCompilerSettingsModel();
      ResetVersionUsedIfRequired();
    }

    private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      llvmController.llvmModel.DownloadProgress = e.ProgressPercentage;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedLlvm"));
    }

    private void ResetVersionUsedIfRequired()
    {
      if (InstalledLlvms.Count == 0)
      {
        VersionUsed = string.Empty;
      }
    }

    private void InsertVersionToInstalledLlvms()
    {
      if (InstalledLlvms.Count == 0)
      {
        InstalledLlvms.Add(llvmController.llvmModel.Version);
        return;
      }

      for (int i = 0; i < InstalledLlvms.Count; i++)
      {
        if (string.CompareOrdinal(llvmController.llvmModel.Version, InstalledLlvms[i]) > 0)
        {
          InstalledLlvms.Insert(i, llvmController.llvmModel.Version);
          break;
        }
      }
    }

    private void DisableButtons(int elementIndex)
    {
      for (int i = 0; i < llvms.Count; i++)
      {
        if (i != elementIndex) llvms[i].CanExecuteCommand = false;
      }
    }

    private void ResetButtonsState()
    {
      foreach (var item in llvms) item.CanExecuteCommand = true;
    }

    #endregion
  }
}
