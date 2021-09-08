using ClangPowerTools;
using ClangPowerTools.MVVM.Command;
using ClangPowerTools.MVVM.Views;
using ClangPowerTools.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Windows.Input;

namespace ClangPowerToolsShared.MVVM.ViewModels
{
  public class FolderExplorerViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    private readonly FolderExplorerView folderExplorerView;
    private string pathFolder = string.Empty;
    private PreinstalledLlvm preinstalledLlvm;
    private List<LlvmModel> llvms = new List<LlvmModel>();

    private ICommand findFolderPathCommand;
    private ICommand downloadLLVMCommand;
    #endregion
    public FolderExplorerViewModel(FolderExplorerView folderExplorer)
    {
      folderExplorerView = folderExplorer;
      PathFolder = string.Empty;
    }

    #region Properties
    public ObservableCollection<string> InstalledLlvms { get; set; } = new ObservableCollection<string>();

    public string PathFolder
    {
      get
      {
        return pathFolder;
      }
      set
      {
        pathFolder = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PathFolder"));
      }
    }
    #endregion

    #region Commands
    public ICommand FindFolderPathCommand
    {
      get => findFolderPathCommand ?? (findFolderPathCommand = new RelayCommand(() => GetFolderPath(), () => CanExecute));
    }
    public ICommand SetLLVMCommand
    {
      get => downloadLLVMCommand ?? (downloadLLVMCommand = new RelayCommand(() => SetLLVM(), () => CanExecute));
    }

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
    #endregion

    #region Public Methods
    public void SetLLVM()
    {
      var clangPath = Path.Combine(PathFolder, "clang.exe");
      if (!File.Exists(clangPath))
      {
        clangPath = Path.Combine(PathFolder, "bin", "clang.exe");

        if (!File.Exists(clangPath))
        {
          MessageBox.Show("LLVM version can't be detected", "Clang Power Tools",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
          return;
        }
        pathFolder = Path.Combine(PathFolder, "bin");
      }

      var versionInfo = FileVersionInfo.GetVersionInfo(clangPath);
      string version = versionInfo.FileVersion.Split()[0];

      preinstalledLlvm = new PreinstalledLlvm(Llvms, InstalledLlvms);
      preinstalledLlvm.SetPreinstalledLlvm(PathFolder, version);
      VersionUsed = version;
      folderExplorerView.Close();
    }

    public void GetFolderPath()
    {

      var llvmBinDirectoryPath = BrowseForFolderFiles();
      PathFolder = llvmBinDirectoryPath;
      if (string.IsNullOrWhiteSpace(llvmBinDirectoryPath))
        return;
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }
    #endregion
  }
}
