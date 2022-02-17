using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Services;
using ClangPowerTools.Views;
using ClangPowerToolsShared.MVVM.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using ClangPowerToolsShared.MVVM.Models;
using ClangPowerToolsShared.MVVM.Models.TidyToolWindowModels;
using EnvDTE80;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerToolsShared.MVVM.ViewModels.ToolWindow
{
  public class TidyToolWindowController
  {
    public ObservableCollection<FileModel> files = new ObservableCollection<FileModel>();
    private CountFilesModel CountFilesModel { get; set; } = new CountFilesModel();
    private List<FileModel> headers = new List<FileModel>();
    private TidyToolWindowView tidyToolWindowView;
    private TidyToolWindowModel tidyToolWindowModel;
    private MessageModel messageModel;
    private string listVisibility = UIElementsConstants.Visibile;
    //To not refresh files value every time (with the same files), and to not refresh check box value

    bool wasMadeTidyOnFiles = false;


    #region Public Methods

    /// <summary>
    /// This method need to be run first.
    /// In this method all values of tidy tool window will be refreshed: files list, check number. After that will be made tidy
    /// </summary>
    /// <param name="filesPath"></param>
    public void InitTidyToolWindow(List<string> filesPath)
    {
      RefreshValues();
      CheckAll();
      TidyFilesAsync(filesPath);
    }

    /// <summary>
    /// Create and add new headers in files list based on file path
    /// </summary>
    /// <param name="filesPath"></param>
    public void AddHeadersInFilesList(List<string> filesPath)
    {
      //if tidy fix was made
      foreach (string file in filesPath)
      {
        FileInfo path = new FileInfo(file);
        if (CheckIsHeader(file))
        {
          var currentHeader = files.Where(a => a.FullFileName == path.FullName).FirstOrDefault();
          if (currentHeader != null)
          {
            currentHeader.IsChecked = true;

            //Remove old header (with disabled diff icon) if already exists in files list, add the new one 
            var index = files.IndexOf(files.Where(f => f.FullFileName == currentHeader.FullFileName).FirstOrDefault());
            if (index > -1)
            {
              files.RemoveAt(index);
            }

            //Add current header on wich was made tidy to files 
            var currentHeaderList = new List<FileModel> { currentHeader };
            MarkFixedFiles(currentHeaderList);
            var currentModelFiles = UnifyFileModelLists(files.ToList(), currentHeaderList);
            files.Clear();
            foreach (var currentFile in currentModelFiles)
            {
              files.Add(new FileModel(currentFile));
            }
          }
        }
      }
      DisableDiffIconForUnfixedHeaders();
      UpdateFiles();
      UpdateCheckedNumber();
    }

    /// <summary>
    /// Refresh values, create and add new fils in files list based on file path
    /// </summary>
    /// <param name="filesPath"></param>
    public void AddFilesInFilesList(List<string> filesPath)
    {
      RefreshValues();
      foreach (string file in filesPath)
      {
        FileInfo path = new FileInfo(file);
        if (CheckIsHeader(file))
        {
          headers.Add(new FileModel { FileName = ". . . " + Path.Combine(path.Directory.Name, path.Name), FullFileName = path.FullName, CopyFullFileName = Path.Combine(TidyConstants.TempsFolderPath, TidyConstants.SolutionTempGuid, GetProjectPathToFile(file)), FilesType = FileType.Header });
        }
        else
        {
          files.Add(new FileModel { FileName = ". . . " + Path.Combine(path.Directory.Name, path.Name), FullFileName = path.FullName, CopyFullFileName = Path.Combine(TidyConstants.TempsFolderPath, TidyConstants.SolutionTempGuid, GetProjectPathToFile(file)), FilesType = FileType.SourceFile });
        }
      }
      CheckAll();
    }

    /// <summary>
    /// Applies tidy fix on all selected source files or just on single file and makes diff
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public async Task FixAllFilesAsync(FileModel file = null)
    {
      //TODO apply tidy fix just on source files, ignore headers

      //if ((tidyToolWindowModel.TotalChecked != tidyToolWindowModel.TotalFixedChecked && files.Where(f => f.FilesType == FileType.File && f.IsChecked && !f.IsFixed).Any()) || file is not null)
      {
        var filesPaths = new List<string>();
        var filesPathsCopy = new List<FileModel>();
        if (file is null)
        {
          //Get checked and unfixed files
          filesPathsCopy = files.Where(f => f.IsChecked && f.IsFixed == false && f.FilesType != FileType.Header).ToList();
          filesPaths = files.Where(f => f.IsChecked && f.FilesType != FileType.Header).Select(f => f.FullFileName).ToList();
        }
        else
        {
          filesPathsCopy = new List<FileModel> { file };
          filesPaths = new List<string> { file.FullFileName };
        }
        FileCommand.CopyFilesInTempSolution(UnifyFileModelLists(filesPathsCopy, headers));
        await CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kTidyFixId, CommandUILocation.ContextMenu, filesPaths);
        MarkFixedFiles(filesPathsCopy);
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

    /// <summary>
    /// Update checked numer on check and uncheck action
    /// </summary>
    /// <param name="file"></param>
    public void UpdateCheckedNumber(FileModel file)
    {
      if (file.IsChecked)
      {
        ++tidyToolWindowModel.TotalChecked;

        tidyToolWindowModel.IsChecked = tidyToolWindowModel.CountFilesModel.TotalChecked == files.Count ? true : false;
      }
      else
      {
        --tidyToolWindowModel.TotalChecked;
        tidyToolWindowModel.IsChecked = tidyToolWindowModel.CountFilesModel.TotalChecked == 0 || tidyToolWindowModel.CountFilesModel.TotalChecked != files.Count ? false : true;
      }
      UpdateTidyToolWindowModelFixedNr();
    }

    public void DiffFile(FileModel file)
    {
      BeforeCommand();
      FileCommand.DiffFilesUsingDefaultTool(FileCommand.GetShortPath(file.CopyFullFileName), FileCommand.GetShortPath(file.FullFileName));
      AfterCommand();
      DisableDiffIconForHeaders();
    }

    #endregion

    #region Private Method

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
    }

    /// <summary>
    /// Set isRunning for every file to make icons disabled 
    /// </summary>
    public void BeforeCommand()
    {
      tidyToolWindowModel.IsRunning = true;
      foreach (var file in files)
      {
        file.IsRunning = true;
      }
    }

    /// <summary>
    /// Set isRunning for every file to make icons disabled 
    /// </summary>
    public void AfterCommand()
    {
      tidyToolWindowModel.IsRunning = false;
      foreach (var file in files)
      {
        file.IsRunning = false;
      }
    }
    private bool CheckIsHeader(string fullFilePath)
    {
      FileInfo path = new FileInfo(fullFilePath);
      return path.FullName.Contains(".h") || path.FullName.Contains(".hpp") || path.FullName.Contains(".hh") || path.FullName.Contains(".hxx");
    }

    private void UpdateTidyToolWindowModelFixedNr()
    {
      //tidyToolWindowModel.FixedNr = 0;
      //tidyToolWindowModel.FixFilesNr = 0;
      //tidyToolWindowModel.TidyFilesNr = 0;
      //tidyToolWindowModel.TotalFixedChecked = 0;
      //foreach (var file in files)
      //{
      //  if (file.IsFixed)
      //  {
      //    ++tidyToolWindowModel.FixedNr;
      //    if (file.IsChecked)
      //      ++tidyToolWindowModel.TotalFixedChecked;
      //  }
      //  if (file.FilesType != FileType.Header && file.IsChecked)
      //  {
      //    ++tidyToolWindowModel.TidyFilesNr;
      //  }
      //  if (file.FilesType != FileType.Header && file.IsChecked && !file.IsFixed)
      //  {
      //    ++tidyToolWindowModel.FixFilesNr;
      //  }
      //}
      //tidyToolWindowModel.TotalCheckedFiles = files.Where(f => f.IsChecked && f.FilesType == FileType.File).Count();
      //tidyToolWindowModel.TotalCheckedFixedFiles = files.Where(f => f.IsChecked && f.FilesType == FileType.File && f.IsFixed).Count();
      //tidyToolWindowModel.TotalCheckedHeaders = files.Where(f => f.IsChecked && f.FilesType == FileType.Header).Count();
    }

    private void CheckAll()
    {
      foreach (var file in files)
      {
        file.IsChecked = true;
      }
      tidyToolWindowModel.IsChecked = true;
      tidyToolWindowModel.CountFilesModel.UpdateTotalChecked(files);
    }

    private void UncheckAll()
    {
      foreach (var file in files)
      {
        file.IsChecked = false;
      }
      tidyToolWindowModel.IsChecked = false;
      tidyToolWindowModel.CountFilesModel.UpdateToUncheckAll();
    }

    /// <summary>
    /// Mark fixed files by adding a dot charcter "•" to the end of file name and
    /// update total number of fixed source files and headers
    /// </summary>
    /// <param name="fixedFiles"></param>
    private void MarkFixedFiles(List<FileModel> fixedFiles)
    {
      foreach (var file in fixedFiles)
      {
        if (!file.IsFixed)
        {
          file.IsFixed = true;
          file.FileName += " •";
          tidyToolWindowModel.CountFilesModel.UpdateFixFileState(file);
        }
      }
    }

    /// <summary>
    /// Mark unfixed files by removing a dot charcter "•" to the end of file name and
    /// update total number of unfixed source files and headers
    /// </summary>
    /// <param name="checkedFiles"></param>
    private void MarkUnfixedFiles(List<FileModel> checkedFiles)
    {
      foreach (var file in checkedFiles)
      {
        if (file.IsFixed)
        {
          file.IsFixed = false;
          file.FileName = file.FileName.Remove(file.FileName.Length - 2, 2);
          tidyToolWindowModel.CountFilesModel.UpdateFixFileState(file);
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
      UpdateTidyToolWindowModelFixedNr();
    }

    /// <summary>
    /// Make diff icon disabled after discard header action
    /// </summary>
    private void DisableDiffIconForHeaders()
    {
      foreach (var file in files)
      {
        if (file.FilesType == FileType.Header && (file.IsChecked || !file.IsFixed))
        {
          file.DisableVisibleDiffIcon();
        }
        if (file.FilesType == FileType.Header && file.IsFixed)
        {
          file.EnableDiffIcon();
        }
      }
      UpdateFiles();
    }


    private void DisableDiffIconForUnfixedHeaders()
    {
      foreach (var file in files)
      {
        if (file.FilesType == FileType.Header && !file.IsFixed)
        {
          file.DisableVisibleDiffIcon();
        }
      }
      UpdateFiles();
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
      if (!files.Where(f => f.FilesType == FileType.SourceFile && f.IsChecked).Any())
      {
        return;
      }
      TidyFilesAsync(paths);
    }

    private async Task TidyFilesAsync(List<string> paths = null)
    {
      BeforeCommand();
      wasMadeTidyOnFiles = true;
      if (paths is null)
      {
        paths = files.Where(f => f.IsChecked && f.FilesType != FileType.Header).Select(f => f.FullFileName).ToList();
      }
      await CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kTidyToolWindowId, CommandUILocation.ContextMenu, paths);
      AfterCommand();
      DisableDiffIconForHeaders();
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
        MarkUnfixedFiles(checkFiles);
        foreach (var file in checkFiles)
        {
          if (file.IsChecked)
          {
            DiscardFile(file);
          }
        }
        //++tidyToolWindowModel.DiscardNr;
        UpdateCheckedNumber();
        AfterCommand();
        DisableDiffIconForHeaders();
      }
    }

    public void ThemeChangeEvent(ThemeChangedEventArgs e)
    {
      tidyToolWindowModel.EnableAllIcons();
      foreach (var file in files)
      {
        file.SelectEnableIcons();
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
    }

    #endregion

  }
}
