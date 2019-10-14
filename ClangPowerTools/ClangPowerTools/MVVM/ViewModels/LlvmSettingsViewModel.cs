using ClangPowerTools.MVVM.Controllers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Threading;

namespace ClangPowerTools
{
  public class LlvmSettingsViewModel : INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    private LlvmController llvmController = new LlvmController();
    private CompilerSettingsModel compilerModel = new CompilerSettingsModel();
    private SettingsProvider settingsProvider = new SettingsProvider();
    private List<LlvmSettingsModel> llvms = new List<LlvmSettingsModel>();
    private CancellationTokenSource downloadCancellationToken = new CancellationTokenSource();
    private bool canUseCommand = true;
    private const string uninstall = "Uninstall";

    #endregion

    #region Constructor
    public LlvmSettingsViewModel()
    {
      llvmController.setInstallCommandState = SetInstallCommandState;
      llvmController.setUninstallCommandState = SetUninstallCommandState;
      llvmController.InstallFinished = InstallFinished;
      llvmController.UninstallFinished = UninstallFinished;
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
      if (canUseCommand)
      {
        canUseCommand = false;
        llvmController.llvmModel = llvms[elementIndex];
        llvmController.llvmModel.IsDownloading = true;
        llvmController.Download(llvmController.llvmModel.Version, DownloadProgressChanged);
      }
    }

    public void CancelCommand(int elementIndex)
    {
      if (llvmController.llvmModel.IsDownloading)
      {
        canUseCommand = true;
        llvmController.llvmModel.DownloadProgress = 0;
        llvmController.llvmModel.IsDownloading = false;
        downloadCancellationToken.Cancel();
      }
    }

    public void UninstallCommand(int elementIndex)
    {
      if (canUseCommand)
      {
        canUseCommand = false;
        llvmController.llvmModel = llvms[elementIndex];
        llvmController.Uninstall(llvmController.llvmModel.Version);
      }
    }

    public void SetInstallCommandState()
    {
      llvmController.llvmModel.IsInstalled = true;
      llvmController.llvmModel.IsInstalling = false;
      canUseCommand = true;
    }

    public void SetUninstallCommandState()
    {
      canUseCommand = true;
      llvmController.llvmModel.IsInstalled = false;
      llvmController.llvmModel.IsDownloading = false;
    }

    public void InstallFinished(object sender, EventArgs e)
    {
#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
      System.Windows.Application.Current.Dispatcher.Invoke(
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs
        new Action(() =>
        {
          InsertVersionToInstalledLlvms();
        }));
    }


    public void UninstallFinished(object sender, EventArgs e)
    {
      ResetVersionUsedIfRequired();
#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
      System.Windows.Application.Current.Dispatcher.Invoke(
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs
        new Action(() =>
        {
          InstalledLlvms.Remove(llvmController.llvmModel.Version);
        }));
    }

    #endregion


    #region Private Methods

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

    #endregion
  }
}
