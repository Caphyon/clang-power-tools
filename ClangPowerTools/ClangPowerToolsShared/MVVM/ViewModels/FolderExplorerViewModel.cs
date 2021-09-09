using ClangPowerTools;
using ClangPowerTools.MVVM.Command;
using ClangPowerTools.MVVM.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;

namespace ClangPowerToolsShared.MVVM.ViewModels
{
  public class FolderExplorerViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;
    private FolderExplorerView folderExplorerView;
    private string pathFolder = string.Empty;
    private PreinstalledLlvm preinstalledLlvm;
    private readonly List<LlvmModel> llvms;
    private ObservableCollection<string> installedLlvms;

    private ICommand findFolderPathCommand;
    private ICommand downloadLLVMCommand;
    #endregion
    public FolderExplorerViewModel(FolderExplorerView folderExplorerView, List<LlvmModel> llvms, ObservableCollection<string> installedLlvms)
    {
      this.llvms = llvms;
      this.installedLlvms = installedLlvms;
      this.folderExplorerView = folderExplorerView;
    }

    #region Properties

    public string PathFolder
    {
      get => pathFolder;
      set
      {
        pathFolder = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PathFolder"));
      }
    }

    public bool CanExecute => true;
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

      if (IsVersionInstalled(version))
      {
        MessageBox.Show("This LLVM is already installed", "Clang Power Tools",
           MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      preinstalledLlvm = new PreinstalledLlvm(llvms, installedLlvms);
      preinstalledLlvm.SetPreinstalledLlvm(PathFolder, version);
      VersionUsed = version;
      folderExplorerView.Close();
    }

    private bool IsVersionInstalled(string version)
    {
      foreach (var llvm in installedLlvms)
      {
        if (llvm == version)
        {
          return true;
        }
      }
      return false;
    }

    public void GetFolderPath()
    {

      var llvmBinDirectoryPath = BrowseForFolderFiles();
      PathFolder = llvmBinDirectoryPath;
      if (string.IsNullOrWhiteSpace(llvmBinDirectoryPath))
        return;
    }

    #endregion
  }
}
