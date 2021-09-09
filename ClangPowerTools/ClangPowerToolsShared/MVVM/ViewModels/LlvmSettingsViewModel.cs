using ClangPowerTools.Handlers;
using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Controllers;
using ClangPowerTools.MVVM.Views;
using ClangPowerTools.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class LlvmSettingsViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;
    public CancelEventHandler WindowClosed;

    private readonly LlvmController llvmController = new LlvmController();
    private List<LlvmModel> llvms = new List<LlvmModel>();
    private PreinstalledLlvm preinstalledLlvm;
    private const string uninstall = "Uninstall";
    private ICommand browseForLlvmCommand;

    private LlvmSettingsView view;


    #endregion

    #region Constructor

    public LlvmSettingsViewModel(LlvmSettingsView view)
    {
      this.view = view;
      llvmController.InstallFinished = InstallFinished;
      llvmController.UninstallFinished = UninstallFinished;
      llvmController.OnOperationCanceldEvent += OperationCanceled;
      WindowClosed += llvmController.SettingsWindowClosed;
      IntitializeView();
      SettingsProvider.Llvms = llvms;
      SettingsProvider.InstalledLlvms = InstalledLlvms;
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

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    #endregion


    #region Commands

    public ICommand BrowseForLLVMCommand
    {
      get => browseForLlvmCommand ??= new RelayCommand(() => BrowseForLLVM(), () => CanExecute);
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

    public void BrowseForLLVM()
    {
      FolderExplorerView folderExplorerview = new FolderExplorerView(Llvms, InstalledLlvms);
      folderExplorerview.ShowDialog();
      VersionUsed = SettingsProvider.LlvmSettingsModel.LlvmSelectedVersion;
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

      preinstalledLlvm = new PreinstalledLlvm(Llvms, InstalledLlvms);
      preinstalledLlvm.SetPreinstalledLlvm();

      SetSelectedVersionIfEmpty();
      ResetVersionUsedIfRequired();
    }

    private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      llvmController.llvmModel.DownloadProgress = e.ProgressPercentage;
    }

    private void SetSelectedVersionIfEmpty()
    {
      if (string.IsNullOrWhiteSpace(VersionUsed))
      {
        if (InstalledLlvms.Count > 0)
          VersionUsed = InstalledLlvms[0];
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
