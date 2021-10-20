using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Command;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Services;
using ClangPowerTools.SilentFile;
using ClangPowerTools.Views;
using ClangPowerToolsShared.MVVM.Commands;
using ClangPowerToolsShared.MVVM.Models;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClangPowerToolsShared.MVVM.ViewModels
{
  public class TidyToolWindowViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;
    private readonly string tempFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClangPowerTools", "Temp");
    private ObservableCollection<FileModel> files = new ObservableCollection<FileModel>();
    private TidyToolWindowView tidyToolWindowView;
    private readonly string folderGuid = Guid.NewGuid().ToString();
    private TidyToolWindowModel tidyToolWindowModel;

    private ICommand tidyAllCommand;
    private ICommand fixAllCommand;
    private ICommand discardAllCommand;
    private ICommand removeAllCommand;
    
    public ObservableCollection<FileModel> Files { get; set; } = new ObservableCollection<FileModel>();

    #endregion

    #region Properties

    public TidyToolWindowModel TidyToolWindowModel
    {
      get {  return tidyToolWindowModel; }
      set
      {
        tidyToolWindowModel = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyToolWindowModel"));
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

    public ICommand TidyAllCommand
    {
      get => tidyAllCommand ?? (tidyAllCommand = new RelayCommand(() => TidyAllFilesAsync().SafeFireAndForget(), () => CanExecute));
    }

    public ICommand FixAllCommand
    {
      get => fixAllCommand ?? (fixAllCommand = new RelayCommand(() => FixAllFilesAsync().SafeFireAndForget(), () => CanExecute));
    }

    public ICommand DiscardAllCommand
    {
      get => discardAllCommand ?? (discardAllCommand = new RelayCommand(() => DiscardAllFiles(), () => CanExecute));
    }

    public ICommand RemoveAllCommand
    {
      get => removeAllCommand ?? (removeAllCommand = new RelayCommand(() => RemoveAllFiles(), () => CanExecute));
    }

    #endregion

    #region Constructos

    public TidyToolWindowViewModel(TidyToolWindowView tidyToolWindowView)
    {
      tidyToolWindowModel = new TidyToolWindowModel();

      tidyToolWindowModel.ButtonVisibility = "Visibile";
      tidyToolWindowModel.ProgressBarVisibility = "Hidden";
      TidyToolWindowModel = tidyToolWindowModel;
      Files = files;
      this.tidyToolWindowView = tidyToolWindowView;
    }

    #endregion

    #region Public Methods

    public void UpdateViewModel(List<string> filesPath)
    {
      files.Clear();
      foreach (string file in filesPath)
      {
        FileInfo path = new FileInfo(file);
        files.Add(new FileModel { FileName = path.Name, FullFileName = path.FullName, CopyFullFileName = Path.Combine(tempFolderPath, folderGuid + "_" + GetProjectPathToFile(file))});
      }
      Files = files;
      //copy files in temp folder
      if (Directory.Exists(tempFolderPath))
        Directory.Delete(tempFolderPath, true);
      Directory.CreateDirectory(tempFolderPath);
      if (Directory.Exists(tempFolderPath))
      {
        FileCommand.CopyFilesInTemp(files.ToList());
      }
    }

    public void DiscardAllFiles()
    {
      BeforeCommand();
      var checkFiles = GetCheckedFiles();
      foreach (var file in checkFiles)
      {
        if (file.IsChecked)
        {
          DiscardFile(file);
        }
      }
      MarkUnfixedFiles();
      AfterCommand();
    }

    public async Task TidyAllFilesAsync()
    {
      BeforeCommand();
      await CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kTidyToolWindowId, CommandUILocation.ContextMenu, GetCheckedPathsList());
      AfterCommand();
    }

    public void RemoveAllFiles()
    {
      BeforeCommand();
      foreach (var file in Files.ToList())
      {
        if (file.IsChecked)
        {
          Files.Remove(file);
        }
      }
      AfterCommand();
    }

    public async Task FixAllFilesAsync()
    {
      BeforeCommand();
      var filesPaths = GetCheckedFiles();
      FileCommand.CopyFilesInTemp(filesPaths);
      await CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kTidyFixId, CommandUILocation.ContextMenu, GetCheckedPathsList());
      MarkFixedFiles();
      AfterCommand();
    }

    public void MarkFixedFile(FileModel currentFile)
    {
      var rfile =  files.Where(f => f.FullFileName == currentFile.FullFileName).SingleOrDefault();
      rfile.IsFixed = true;
      Files = files;
    }

    #endregion

    #region Private Method

    private void MarkFixedFiles()
    {
      var checkedFiles = GetCheckedFiles();
      foreach (var file in checkedFiles)
      {
        file.IsFixed = true;
      }
    }

    private void MarkUnfixedFiles()
    {
      var checkedFiles = GetCheckedFiles();
      foreach (var file in checkedFiles)
      {
        file.IsFixed = false;
      }
    }

    private List<FileModel> GetCheckedFiles()
    {
      return  files.Where(f => f.IsChecked).ToList();
    }

    private List<string> GetCheckedPathsList()
    {
      var pathList = new List<string>();
      foreach (var path in files)
      {
        if (path.IsChecked)
          pathList.Add(path.FullFileName);
      }
      return pathList;
    }

    private string GetProjectPathToFile(string file)
    {
      FileInfo path = new FileInfo(file);
      string directoryName = path.Directory.Name;
      var fullFileName = path.FullName;
      var index = fullFileName.IndexOf(directoryName);
      return fullFileName.Substring(index, fullFileName.Length - index); ;
    }

    private void DiscardFile(FileModel file)
    {
      var fileChangerWatcher = new FileChangerWatcher();

      var dte2 = VsServiceProvider.GetService(typeof(DTE2)) as DTE2;
      string solutionFolderPath = SolutionInfo.IsOpenFolderModeActive() ?
        dte2.Solution.FullName : dte2.Solution.FullName
                                  .Substring(0, dte2.Solution.FullName.LastIndexOf('\\'));
      fileChangerWatcher.Run(solutionFolderPath);

      if (File.Exists(file.CopyFullFileName))
      {
        File.Copy(file.CopyFullFileName, file.FullFileName, true);
        File.Delete(file.CopyFullFileName);
      }
    }

    private void BeforeCommand()
    {
      tidyToolWindowModel.IsRunning = true;
      TidyToolWindowModel = tidyToolWindowModel;
    }

    private void AfterCommand()
    {
      TidyToolWindowModel.IsRunning = false;
      TidyToolWindowModel = tidyToolWindowModel;
    }

    #endregion

  }
}
