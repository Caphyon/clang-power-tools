using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Command;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Services;
using ClangPowerTools.Views;
using ClangPowerToolsShared.MVVM.Commands;
using ClangPowerToolsShared.MVVM.Models;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Task = System.Threading.Tasks.Task;

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
      get { return tidyToolWindowModel; }
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
      RefreshValues();
      foreach (string file in filesPath)
      {
        FileInfo path = new FileInfo(file);
        files.Add(new FileModel { FileName = path.Name, FullFileName = path.FullName, CopyFullFileName = Path.Combine(tempFolderPath, folderGuid + "_" + GetProjectPathToFile(file)) });
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

    public void CheckOrUncheckAll()
    {
      if (tidyToolWindowModel.IsChecked)
      {
        CheckAll();
      }
      else
      {
        UncheckAll();
      }
      UpdateCheckedNumber();
    }

    public async Task FixAllFilesAsync(FileModel file = null)
    {
      BeforeCommand();
      var filesPaths = new List<string>();
      var filesPathsCopy = new List<FileModel>();
      if (file is null)
      {
        filesPathsCopy = GetCheckedFiles();
        filesPaths = GetCheckedPathsList();
      }
      else
      {
        filesPathsCopy = new List<FileModel> { file };
        filesPaths = new List<string> { file.FullFileName };
      }
      FileCommand.CopyFilesInTemp(filesPathsCopy);
      await CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kTidyFixId, CommandUILocation.ContextMenu, filesPaths);
      MarkFixedFiles(filesPathsCopy);
      AfterCommand();
    }

    public void UpdateCheckedNumber(FileModel file)
    {
      if (file.IsChecked)
      {
        ++tidyToolWindowModel.TotalChecked;
        tidyToolWindowModel.IsChecked = tidyToolWindowModel.TotalChecked == files.Count ? true : false;
        TidyToolWindowModel = tidyToolWindowModel;
      }
      else
      {
        --tidyToolWindowModel.TotalChecked;
        tidyToolWindowModel.IsChecked = tidyToolWindowModel.TotalChecked == 0 || tidyToolWindowModel.TotalChecked != files.Count ? false : true;
        TidyToolWindowModel = tidyToolWindowModel;
      }
    }

    public void DiffFile(FileModel file)
    {
      BeforeCommand();
      FileCommand.DiffFilesUsingDefaultTool(file.CopyFullFileName, file.FullFileName);
      AfterCommand();
    }

    #endregion

    #region Private Method

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

    private void CheckAll()
    {
      foreach (var file in files)
      {
        file.IsChecked = true;
      }
      Files = files;
    }

    private void UncheckAll()
    {
      foreach (var file in files)
      {
        file.IsChecked = false;
      }
      Files = files;
    }

    private void MarkFixedFiles(List<FileModel> fixedFiles)
    {
      foreach (var file in fixedFiles)
      {
        file.IsFixed = true;
      }
      Files = files;
    }

    private void MarkUnfixedFiles(List<FileModel> checkedFiles)
    {
      foreach (var file in checkedFiles)
      {
        file.IsFixed = false;
      }
      Files = files;
    }

    private List<FileModel> GetCheckedFiles()
    {
      return files.Where(f => f.IsChecked).ToList();
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

    private void RemoveAllFiles()
    {
      BeforeCommand();
      foreach (var file in Files.ToList())
      {
        if (file.IsChecked)
        {
          Files.Remove(file);
        }
      }
      UpdateCheckedNumber();
      AfterCommand();
    }

    private async Task TidyAllFilesAsync()
    {
      BeforeCommand();
      await CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kTidyToolWindowId, CommandUILocation.ContextMenu, GetCheckedPathsList());
      AfterCommand();
    }

    private void UpdateCheckedNumber()
    {
      tidyToolWindowModel.TotalChecked = 0;
      foreach (var file in files)
      {
        if (file.IsChecked)
        {
          ++tidyToolWindowModel.TotalChecked;
        }
      }
      tidyToolWindowModel.IsChecked = tidyToolWindowModel.TotalChecked is 0 ? false : true;
      TidyToolWindowModel = tidyToolWindowModel;
    }

    private void DiscardAllFiles()
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
      MarkUnfixedFiles(checkFiles);
      ++tidyToolWindowModel.DiscardNr;
      AfterCommand();
    }

    private void RefreshValues()
    {
      tidyToolWindowModel.TotalChecked = 0;
      tidyToolWindowModel.IsChecked = false;
      TidyToolWindowModel = tidyToolWindowModel;
      files.Clear();
    }

    #endregion

  }
}
