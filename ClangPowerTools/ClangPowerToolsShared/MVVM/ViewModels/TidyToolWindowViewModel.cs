using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Command;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Services;
using ClangPowerTools.Views;
using ClangPowerToolsShared.MVVM.Commands;
using ClangPowerToolsShared.MVVM.Constants;
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
    private ObservableCollection<FileModel> files = new ObservableCollection<FileModel>();
    private TidyToolWindowView tidyToolWindowView;
    private TidyToolWindowModel tidyToolWindowModel;
    private MessageModel messageModel;
    private string listVisibility = UIElementsConstants.Visibile;

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

    public MessageModel MessageModel
    {
      get { return messageModel; }
      set
      {
        messageModel = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MessageModel"));
      }
    }

    public string ListVisibility
    {
      get { return listVisibility; }
      set
      {
        listVisibility = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ListVisibility"));
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
      messageModel = new MessageModel();

      tidyToolWindowModel.ButtonVisibility = UIElementsConstants.Visibile;
      tidyToolWindowModel.ProgressBarVisibility = UIElementsConstants.Hidden;
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
        files.Add(new FileModel { FileName = ". . . " + Path.Combine(path.Directory.Name, path.Name), FullFileName = path.FullName, CopyFullFileName = Path.Combine(TidyConstants.TempsFolderPath, TidyConstants.SolutionTempGuid, GetProjectPathToFile(file)) });
      }
      Files = files;
      CheckAll();
      //make tidy
      TidyAllFilesAsync();
      //copy files in temp folder
      if (!Directory.Exists(TidyConstants.LongFilePrefix + TidyConstants.TempsFolderPath))
        Directory.CreateDirectory(TidyConstants.LongFilePrefix + TidyConstants.TempsFolderPath);
      if (Directory.Exists(TidyConstants.LongFilePrefix + TidyConstants.TempsFolderPath))
      {
        FileCommand.CopyFilesInTempSolution(files.ToList());
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
      FileCommand.CopyFilesInTempSolution(filesPathsCopy);
      await CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kTidyFixId, CommandUILocation.ContextMenu, filesPaths);
      if (file is not null)
      {
        DiffFile(file);
      }
      MarkFixedFiles(filesPathsCopy);
      UpdateTidyToolWindowModelFixedNr();
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
      FileCommand.DiffFilesUsingDefaultTool(FileCommand.GetShortPath(file.CopyFullFileName), FileCommand.GetShortPath(file.FullFileName));
      AfterCommand();
    }

    #endregion

    #region Private Method

    private void BeforeCommand()
    {
      tidyToolWindowModel.IsRunning = true;
      foreach (var file in files)
      {
        file.IsRunning = true;
      }
      Files = files;
      TidyToolWindowModel = tidyToolWindowModel;
    }

    private void AfterCommand()
    {
      TidyToolWindowModel.IsRunning = false;
      foreach (var file in files)
      {
        file.IsRunning = false;
      }
      Files = files;
      TidyToolWindowModel = tidyToolWindowModel;
    }

    private void UpdateTidyToolWindowModelFixedNr()
    {
      tidyToolWindowModel.FixedNr = 0;
      foreach (var file in files)
      {
        if (file.IsFixed)
          ++tidyToolWindowModel.FixedNr;
      }
    }

    private void CheckAll()
    {
      tidyToolWindowModel.IsChecked = true;
      foreach (var file in files)
      {
        file.IsChecked = true;
      }
      tidyToolWindowModel.TotalChecked = files.Count;
      Files = files;
      TidyToolWindowModel = tidyToolWindowModel;
    }

    private void UncheckAll()
    {
      tidyToolWindowModel.IsChecked = false;
      foreach (var file in files)
      {
        file.IsChecked = false;
      }
      tidyToolWindowModel.TotalChecked = 0;
      Files = files;
      TidyToolWindowModel = tidyToolWindowModel;
    }

    private void MarkFixedFiles(List<FileModel> fixedFiles)
    {
      foreach (var file in fixedFiles)
      {
        file.IsFixed = true;
      }
    }

    private void MarkUnfixedFiles(List<FileModel> checkedFiles)
    {
      foreach (var file in checkedFiles)
      {
        file.IsFixed = false;
      }
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

      if (File.Exists(TidyConstants.LongFilePrefix + file.CopyFullFileName))
      {
        File.Copy(TidyConstants.LongFilePrefix + file.CopyFullFileName, TidyConstants.LongFilePrefix + file.FullFileName, true);
        File.Delete(TidyConstants.LongFilePrefix + file.CopyFullFileName);
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
      if (Files.Count == 0)
      {
        listVisibility = UIElementsConstants.Hidden;
        ListVisibility = ListVisibility;
        messageModel.Visibility = UIElementsConstants.Visibile;
        messageModel.TextMessage = "You don't have any files, run tidy to add files";
        MessageModel = messageModel;
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
      files.Clear();
      listVisibility = UIElementsConstants.Visibile;
      messageModel.Visibility = UIElementsConstants.Hidden;
      ListVisibility = listVisibility;
      MessageModel = messageModel;
      tidyToolWindowModel.TotalChecked = 0;
      tidyToolWindowModel.IsChecked = false;
      TidyToolWindowModel = tidyToolWindowModel;
    }

    #endregion

  }
}
