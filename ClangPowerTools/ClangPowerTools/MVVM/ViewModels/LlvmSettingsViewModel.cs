using ClangPowerTools.Handlers;
using ClangPowerTools.MVVM.Controllers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace ClangPowerTools
{
  public class LlvmSettingsViewModel : INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;
    public CancelEventHandler WindowClosed;

    private readonly LlvmController llvmController = new LlvmController();
    private List<LlvmModel> llvms = new List<LlvmModel>();
    private const string uninstall = "Uninstall";

    #endregion

    #region Constructor

    public LlvmSettingsViewModel()
    {
      llvmController.InstallFinished = InstallFinished;
      llvmController.UninstallFinished = UninstallFinished;
      llvmController.OnOperationCanceldEvent += OperationCanceled;
      WindowClosed += llvmController.SettingsWindowClosed;
      IntitializeView();
    }
    #endregion

    #region Properties

    public List<LlvmModel> Llvms
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
        return SettingsProvider.LlvmSettingsModel.LlvmSelectedVersion;
      }

      set
      {
        SettingsProvider.LlvmSettingsModel.LlvmSelectedVersion = value;
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
      VersionUsed = llvmController.llvmModel.Version;
      UIUpdater.InvokeAsync(InsertVersionToInstalledLlvms).SafeFireAndForget();
    }


    private void UninstallFinished(object sender, EventArgs e)
    {
      ResetVersionUsedIfRequired();
      ResetButtonsState();
      UIUpdater.InvokeAsync(new Action(() =>
      {
        InstalledLlvms.Remove(llvmController.llvmModel.Version);
        if (InstalledLlvms.Count > 0 && InstalledLlvms.Contains(VersionUsed) == false)
        {
          VersionUsed = InstalledLlvms[0];
        }
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
        var llvmModel = new LlvmModel()
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
      SetPreinstalledLllvm();
      SetSelectedVersionIfEmpty();
      ResetVersionUsedIfRequired();
    }

    private void SetPreinstalledLllvm()
    {
      var llvmSettingsModel = SettingsProvider.LlvmSettingsModel;

      GetPathAndVersion(out string path, out string version);
      if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(version)) return;

      SetPathAndVersion(path, version);
      if (Directory.Exists(llvmSettingsModel.PreinstalledLlvmPath) == false)
      {
        llvmSettingsModel.PreinstalledLlvmPath = string.Empty;
        llvmSettingsModel.PreinstalledLlvmVersion = string.Empty;
        return;
      }

      InstalledLlvms.Add(llvmSettingsModel.PreinstalledLlvmVersion);
    }

    private void SetPathAndVersion(string path, string version)
    {
      var settingsProviderLlvmModel = SettingsProvider.LlvmSettingsModel;
      if (string.IsNullOrWhiteSpace(settingsProviderLlvmModel.PreinstalledLlvmVersion)
        || string.IsNullOrWhiteSpace(settingsProviderLlvmModel.PreinstalledLlvmPath))
      {
        var llvmModel = llvms.Find(e => e.Version == version);
        llvmModel.HasPreinstalledLlvm = true;
        llvmModel.PreinstalledLlvmPath = path;

        settingsProviderLlvmModel.PreinstalledLlvmVersion = version;
        settingsProviderLlvmModel.PreinstalledLlvmPath = path;
      }
      else
      {
        var llvmModel = llvms.Find(e => e.Version == settingsProviderLlvmModel.PreinstalledLlvmVersion);
        llvmModel.HasPreinstalledLlvm = true;
        llvmModel.PreinstalledLlvmPath = settingsProviderLlvmModel.PreinstalledLlvmPath;
      }
    }

    private void GetPathAndVersion(out string path, out string version)
    {
      var llvmSettingsModel = SettingsProvider.LlvmSettingsModel;
      if (InstalledLlvms.Count == 0)
      {
        path = llvmController.GetLlvmPathFromRegistry();
        version = llvmController.GetVersionFromRegistry();
        llvmSettingsModel.LlvmSelectedVersion = version;
      }
      else
      {
        path = llvmSettingsModel.PreinstalledLlvmPath;
        version = llvmSettingsModel.LlvmSelectedVersion;
      }
    }

    private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      llvmController.llvmModel.DownloadProgress = e.ProgressPercentage;
    }

    private void SetSelectedVersionIfEmpty()
    {
      
      if(string.IsNullOrWhiteSpace(VersionUsed))
      {
        if (llvms.Count > 0)
          VersionUsed = llvms[0].Version;
      }
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
      for (int i = 0; i < InstalledLlvms.Count; i++)
      {
        if (string.CompareOrdinal(llvmController.llvmModel.Version, InstalledLlvms[i]) > 0)
        {
          InstalledLlvms.Insert(i, llvmController.llvmModel.Version);
          return;
        }
      }
      InstalledLlvms.Add(llvmController.llvmModel.Version);
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
      foreach (var item in llvms)
        item.CanExecuteCommand = true;
    }

    #endregion
  }
}
