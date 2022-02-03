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
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerToolsShared.MVVM.ViewModels
{
  public class TidyToolWindowViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;
    private ObservableCollection<FileModel> files = new ObservableCollection<FileModel>();
    private List<FileModel> headers = new List<FileModel>();
    private TidyToolWindowView tidyToolWindowView;
    private TidyToolWindowModel tidyToolWindowModel;
    private MessageModel messageModel;
    private string listVisibility = UIElementsConstants.Visibile;
    //To not refresh files value every time (with the same files), and to not refresh check box value
    bool filesAlreadyExists = false;
    bool wasMadeTidyOnFiles = false;

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
      VSColorTheme.ThemeChanged += ThemeChangeEvent;
      messageModel = new MessageModel();

      tidyToolWindowModel.ButtonVisibility = UIElementsConstants.Visibile;
      tidyToolWindowModel.ProgressBarVisibility = UIElementsConstants.Hidden;
      TidyToolWindowModel = tidyToolWindowModel;
      UpdateFiles();
      this.tidyToolWindowView = tidyToolWindowView;
      CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(Files);
      PropertyGroupDescription groupDescription = new PropertyGroupDescription("FilesType");
      view.GroupDescriptions.Add(groupDescription);
    }

    #endregion

    #region Public Methods

    public void OpenTidyToolWindow(List<string> filesPath)
    {
      RefreshValues();
      CheckAll();
      TidyAllFilesAsync(filesPath);
      filesAlreadyExists = false;
    }

    public void UpdateViewModel(List<string> filesPath)
    {
      //if tidy fix was made
      if (!wasMadeTidyOnFiles)
      {
        foreach (string file in filesPath)
        {
          FileInfo path = new FileInfo(file);
          if (path.FullName.Contains(".h") || path.FullName.Contains(".hpp") || path.FullName.Contains(".hh") || path.FullName.Contains(".hxx"))
          {
            //TODO check if current header is null
            var currentHeaders = headers.Where(a => a.FullFileName == path.FullName).FirstOrDefault();
            var test = files.Where(a => a.FullFileName == currentHeaders.FullFileName).FirstOrDefault();
            if (test is null)
            {
              //add current header on wich was made tidy to files 
              var currentModelFiles = UnifyFileModelLists(files.ToList(), new List<FileModel> { new FileModel(currentHeaders) });
              files.Clear();
              foreach(var currentFile in currentModelFiles)
              {
                files.Add(new FileModel(currentFile));
              }
              UpdateFiles();
               
            }

            //if(currentHeaders == null)
            //{
            //  currentHeaders = new FileModel { FileName = ". . . " + Path.Combine(path.Directory.Name, path.Name), FullFileName = path.FullName, CopyFullFileName = Path.Combine(TidyConstants.TempsFolderPath, TidyConstants.SolutionTempGuid, GetProjectPathToFile(file)), FilesType = FileType.File };
            //  files.Add(currentHeaders);
            //}
            currentHeaders.IsChecked = true;
            MarkFixedFiles(new List<FileModel> { currentHeaders });
          }
        }
        UpdateCheckedNumber();
      }
      if (!filesAlreadyExists)
      {
        RefreshValues();
        foreach (string file in filesPath)
        {
          FileInfo path = new FileInfo(file);

          if (path.FullName.Contains(".h") || path.FullName.Contains(".hpp") || path.FullName.Contains(".hh") || path.FullName.Contains(".hxx"))
          {
            headers.Add(new FileModel { FileName = ". . . " + Path.Combine(path.Directory.Name, path.Name), FullFileName = path.FullName, CopyFullFileName = Path.Combine(TidyConstants.TempsFolderPath, TidyConstants.SolutionTempGuid, GetProjectPathToFile(file)), FilesType = FileType.Header });
          }
          else
          {
            files.Add(new FileModel { FileName = ". . . " + Path.Combine(path.Directory.Name, path.Name), FullFileName = path.FullName, CopyFullFileName = Path.Combine(TidyConstants.TempsFolderPath, TidyConstants.SolutionTempGuid, GetProjectPathToFile(file)), FilesType = FileType.File });
          }
        }
        CheckAll();
        SaveLastUpdatesToUI();
        filesAlreadyExists = true;

      }
      
      if (!Directory.Exists(TidyConstants.TempsFolderPath))
        Directory.CreateDirectory(TidyConstants.TempsFolderPath);

    }

    public void CheckOrUncheckAll()
    {
      if (tidyToolWindowModel.IsChecked)
      {
        CheckAll();
        SaveLastUpdatesToUI();
      }
      else
      {
        UncheckAll();
      }
      UpdateCheckedNumber();
    }

    public async Task FixAllFilesAsync(FileModel file = null)
    {
      UpdateTidyToolWindowModelFixedNr();
      if (tidyToolWindowModel.TotalChecked != tidyToolWindowModel.TotalFixedChecked || file is not null)
      {
        BeforeCommand();
        var filesPaths = new List<string>();
        var filesPathsCopy = new List<FileModel>();
        if (file is null)
        {
          //get checked and unfixed files
          filesPathsCopy = files.Where(f => f.IsChecked && f.IsFixed == false).ToList();
          filesPaths = Files.Where(f => f.IsChecked).Select(f => f.FullFileName).ToList();
        }
        else
        {
          filesPathsCopy = new List<FileModel> { file };
          filesPaths = new List<string> { file.FullFileName };
        }
        FileCommand.CopyFilesInTempSolution(UnifyFileModelLists(filesPathsCopy, headers));
        await CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kTidyFixId, CommandUILocation.ContextMenu, filesPaths);
        wasMadeTidyOnFiles = false;
        if (file is not null)
        {
          DiffFile(file);
        }
        MarkFixedFiles(filesPathsCopy);
        UpdateCheckedNumber();
        UpdateFiles();
        AfterCommand();
      }
    }

    private List<FileModel> UnifyFileModelLists(List<FileModel> firstList, List<FileModel> secondList)
    {
      var fileUnion = new List<FileModel>();
      foreach (var file in firstList)
      {
        fileUnion.Add(file);
      }
      foreach (var file in secondList)
      {
        fileUnion.Add(file);
      }
      return fileUnion;
    }

    public void UpdateCheckedNumber(FileModel file)
    {
      if (file.IsChecked)
      {
        ++tidyToolWindowModel.TotalChecked;
        UpdateTidyToolWindowModelFixedNr();
        tidyToolWindowModel.IsChecked = tidyToolWindowModel.TotalChecked == files.Count ? true : false;
        TidyToolWindowModel = tidyToolWindowModel;
      }
      else
      {
        --tidyToolWindowModel.TotalChecked;
        UpdateTidyToolWindowModelFixedNr();
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
      UpdateFiles();
      TidyToolWindowModel = tidyToolWindowModel;
    }

    private void AfterCommand()
    {
      TidyToolWindowModel.IsRunning = false;
      foreach (var file in files)
      {
        file.IsRunning = false;
      }
      UpdateFiles();
      TidyToolWindowModel = tidyToolWindowModel;
    }

    private void UpdateTidyToolWindowModelFixedNr()
    {
      tidyToolWindowModel.FixedNr = 0;
      tidyToolWindowModel.TotalFixedChecked = 0;
      foreach (var file in files)
      {
        if (file.IsFixed)
        {
          ++tidyToolWindowModel.FixedNr;
          if (file.IsChecked)
            ++tidyToolWindowModel.TotalFixedChecked;
        }
      }
    }

    private void CheckAll()
    {
      tidyToolWindowModel.IsChecked = true;
      tidyToolWindowModel.TotalFixedChecked = 0;
      foreach (var file in files)
      {
        file.IsChecked = true;
        if (file.IsFixed)
          ++tidyToolWindowModel.TotalChecked;
      }
      tidyToolWindowModel.TotalChecked = files.Count;
      UpdateCheckedNumber();
      SaveLastUpdatesToUI();
    }

    private void SaveLastUpdatesToUI()
    {
      UpdateFiles();
      TidyToolWindowModel = tidyToolWindowModel;
    }

    private void UncheckAll()
    {
      tidyToolWindowModel.IsChecked = false;
      foreach (var file in files)
      {
        file.IsChecked = false;
      }
      tidyToolWindowModel.TotalFixedChecked = 0;
      tidyToolWindowModel.TotalChecked = 0;
      UpdateCheckedNumber();
      SaveLastUpdatesToUI();
    }

    private void MarkFixedFiles(List<FileModel> fixedFiles)
    {
      foreach (var file in fixedFiles)
      {
        if (!file.IsFixed)
        {
          file.IsFixed = true;
          file.FileName += " •";
        }
      }
    }

    private void MarkUnfixedFiles(List<FileModel> checkedFiles)
    {
      foreach (var file in checkedFiles)
      {
        if (file.IsFixed)
        {
          file.IsFixed = false;
          file.FileName = file.FileName.Remove(file.FileName.Length - 2, 2);
        }
      }
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
        try
        {
          File.Copy(file.CopyFullFileName, file.FullFileName, true);
          File.Delete(file.CopyFullFileName);
        }
        catch (UnauthorizedAccessException e)
        {
          MessageBox.Show($"Access to path {file.FullFileName} is denied", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

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

    private async Task TidyAllFilesAsync(List<string> paths = null)
    {
      BeforeCommand();
      wasMadeTidyOnFiles = true;
      if (paths is null)
      {
        paths = files.Where(f => f.IsChecked).Select(f => f.FullFileName).ToList();
      }
      await CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kTidyToolWindowId, CommandUILocation.ContextMenu, paths);
      AfterCommand();
    }

    private void UpdateCheckedNumber()
    {
      UpdateTidyToolWindowModelFixedNr();
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
      if (TidyToolWindowModel.TotalFixedChecked != 0)
      {
        BeforeCommand();
        var checkFiles = files.Where(f => f.IsChecked).ToList();
        foreach (var file in checkFiles)
        {
          if (file.IsChecked)
          {
            DiscardFile(file);
          }
        }
        MarkUnfixedFiles(checkFiles);
        ++tidyToolWindowModel.DiscardNr;
        UpdateCheckedNumber();
        AfterCommand();
      }
    }

    public void ThemeChangeEvent(ThemeChangedEventArgs e)
    {
      tidyToolWindowModel.EnableAllIcons();
      foreach (var file in files)
      {
        file.EnableIcon();
      }
      SaveLastUpdatesToUI();
    }

    private void RefreshValues()
    {
      files.Clear();
      listVisibility = UIElementsConstants.Visibile;
      messageModel.Visibility = UIElementsConstants.Hidden;
      ListVisibility = listVisibility;
      MessageModel = messageModel;
      tidyToolWindowModel.TotalChecked = 0;
      tidyToolWindowModel.DisableDiscardFixIcon();
      tidyToolWindowModel.TotalFixedChecked = 0;
      tidyToolWindowModel.IsChecked = false;
      SaveLastUpdatesToUI();
    }

    private void UpdateFiles()
    {
      Files = files;
      //var resultFiles = files.Where(f => f.FileName == "").ToList();
      //var resultFiles = files.Where(f => f.FileName == "").ToList();
      //ObservableCollection<FileModel> fileModels = new ObservableCollection<FileModel>();
      //foreach (var file in resultFiles)
      //{
      //  Files.Remove(file);
      //}
    }

    #endregion

  }
}