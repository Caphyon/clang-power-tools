using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Services;
using ClangPowerToolsShared.MVVM.Commands;
using ClangPowerToolsShared.MVVM.Models.TidyToolWindowModels;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerToolsShared.MVVM.Controllers
{
  public class TidyToolWindowController
  {
    #region Properties

    public ObservableCollection<FileModel> files = new ObservableCollection<FileModel>();
    private List<FileModel> headers = new List<FileModel>();
    public TidyToolWindowModel tidyToolWindowModel = new TidyToolWindowModel();

    #endregion

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
      List<FileModel> resultHeadersList = new List<FileModel>();
      foreach (string file in filesPath)
      {
        FileInfo path = new FileInfo(file);
        if (CheckIsHeader(file))
        {
          var currentHeader = headers.Where(a => a.FullFileName == path.FullName).FirstOrDefault();
          if (currentHeader != null)
          {
            currentHeader.IsChecked = true;
            UpdateCheckedNumber(currentHeader);

            //Remove old header (with disabled diff icon) if already exists in files list, add the new one 
            var index = files.IndexOf(files.Where(f => f.FullFileName == currentHeader.FullFileName).FirstOrDefault());
            if (index > -1)
            {
              files.RemoveAt(index);
            }
            //Add current header on wich was made tidy to files 
            MarkFixedFiles(new List<FileModel> { currentHeader });
            resultHeadersList.Add(currentHeader);
          }
        }
      }
      var result = UnifyFileModelLists(files.ToList(), resultHeadersList);
      files.Clear();
      foreach (var currentFile in result)
      {
        files.Add(new FileModel(currentFile));
      }
      UpdateTidyToolWindowCheckBox();
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
          headers.Add(new FileModel { FileName = ". . . " + Path.Combine(path.Directory.Name, path.Name),
            FullFileName = path.FullName,
            CopyFullFileName = Path.Combine(TidyConstants.TempsFolderPath,
            TidyConstants.SolutionTempGuid, Guid.NewGuid().ToString()) + path.Extension,
            FilesType = FileType.Header });
        }
        else
        {
          files.Add(new FileModel { FileName = ". . . " + Path.Combine(path.Directory.Name, path.Name),
            FullFileName = path.FullName,
            CopyFullFileName = Path.Combine(TidyConstants.TempsFolderPath,
            TidyConstants.SolutionTempGuid, Guid.NewGuid().ToString()) + path.Extension,
            FilesType = FileType.SourceFile });
        }
      }
      CheckAll();
    }

    private string GetExtension(string path)
    {
      Regex regex = new Regex(@"([A-Z]:\\.+?\.(cpp|cu|cc|cp|tlh|c|cxx|tli|h|hh|hpp|hxx))(\W|$)");
      Match match = regex.Match(path);
      return match.Value;
    }

    /// <summary>
    /// Applies tidy fix on all selected source files or just on single file and makes diff after
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public async Task FixAllFilesAsync(FileModel file = null)
    {
      //TODO apply tidy fix just on source files, ignore headers
      BeforeCommand();
      //if ((tidyToolWindowModel.TotalChecked != tidyToolWindowModel.TotalFixedChecked && files.Where(f => f.FilesType == FileType.File && f.IsChecked && !f.IsFixed).Any()) || file is not null)
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
        if (!file.IsChecked)
        {
          file.IsChecked = true;
          UpdateCheckedNumber(file);
        }
        filesPathsCopy = new List<FileModel> { file };
        filesPaths = new List<string> { file.FullFileName };
      }
      FileCommand.CopyFilesInTempSolution(UnifyFileModelLists(filesPathsCopy, headers));
      await CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kTidyFixId, CommandUILocation.ContextMenu, filesPaths);
      MarkFixedFiles(filesPathsCopy);
      if (file is not null)
      {
        DiffBetweenCopyAndCurrent(file);
      }
      AfterCommand();
    }

    /// <summary>
    /// Update checked numer on check and uncheck action
    /// </summary>
    /// <param name="file"></param>
    public void UpdateCheckedNumber(FileModel file)
    {
      if (file.IsChecked)
      {
        //Check "global" checkbox if all files are checked
        tidyToolWindowModel.CountFilesModel.CheckFileUpdate(file);
        tidyToolWindowModel.IsChecked = tidyToolWindowModel.CountFilesModel.TotalCheckedFiles == files.Count ? true : false;
      }
      else
      {
        //Uncheck "global" checkbox if no file is checked
        tidyToolWindowModel.CountFilesModel.UnCheckFileUpdate(file);
        tidyToolWindowModel.IsChecked = tidyToolWindowModel.CountFilesModel.TotalCheckedFiles == 0 ||
          tidyToolWindowModel.CountFilesModel.TotalCheckedFiles != files.Count ? false : true;
      }
    }

    /// <summary>
    /// Update Tidy Tool Window checkbox
    /// </summary>
    public void UpdateTidyToolWindowCheckBox()
    {
      tidyToolWindowModel.IsChecked = tidyToolWindowModel.CountFilesModel.TotalCheckedFiles == files.Count ? true : false;
      tidyToolWindowModel.IsChecked = tidyToolWindowModel.CountFilesModel.TotalCheckedFiles == 0 ||
        tidyToolWindowModel.CountFilesModel.TotalCheckedFiles != files.Count ? false : true;
    }


    /// <summary>
    /// Make a diff betweeen copy file and current file
    /// </summary>
    /// <param name="file"></param>
    public void DiffBetweenCopyAndCurrent(FileModel file)
    {
      BeforeCommand();
      FileCommand.DiffFilesUsingDefaultTool(FileCommand.GetShortPath(file.CopyFullFileName), FileCommand.GetShortPath(file.FullFileName));
      AfterCommand();
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

    /// <summary>
    /// Mark fixed files by adding a dot charcter "•" to the end of file name and
    /// update total number of fixed source files and headers
    /// </summary>
    /// <param name="fixedFiles"></param>
    public void MarkFixedFiles(List<FileModel> fixedFiles)
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
    /// Replace current file on which was made tidy fix with a copy
    /// </summary>
    /// <param name="file"></param>
    public void DiscardFile(FileModel file)
    {
      var fileChangerWatcher = new FileChangerWatcher();

      var dte2 = VsServiceProvider.GetService(typeof(DTE2)) as DTE2;
      string solutionFolderPath = SolutionInfo.IsOpenFolderModeActive() ?
        dte2.Solution.FullName : dte2.Solution.FullName
                                  .Substring(0, dte2.Solution.FullName.LastIndexOf('\\'));
      fileChangerWatcher.Run(solutionFolderPath);
      FileCommand.CopyFileFromTempToSolution(file);
      MarkUnfixedFiles(file);
    }


    /// <summary>
    /// Remove selected files from files list
    /// </summary>
    public void RemoveFiles(FileModel customFile = null)
    {
      BeforeCommand();
      if (customFile is not null)
      {
        //Remove file from list
        var removeFile = files.Where(f => f.IsChecked && f.FullFileName == customFile.FullFileName).SingleOrDefault();
        if (removeFile is not null)
        {
          //Mark as checked, and restore to initial properties to be removed after
          removeFile.IsChecked = false;
          UpdateCheckedNumber(removeFile);
          removeFile.IsChecked = true;
          files.Remove(removeFile);
        }
      }
      else
      {
        foreach (var file in files.ToList())
        {
          if (file.IsChecked)
          {
            //Mark as checked, and restore to initial properties to be removed after
            file.IsChecked = false;
            UpdateCheckedNumber(file);
            file.IsChecked = true;
            files.Remove(file);
          }
        }
      }
      UpdateTidyToolWindowCheckBox();
      AfterCommand();
    }

    public async Task TidyFilesAsync(List<string> paths = null)
    {
      BeforeCommand();
      if (paths is null)
      {
        paths = files.Where(f => f.IsChecked && f.FilesType != FileType.Header).Select(f => f.FullFileName).ToList();
      }
      await CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kTidyToolWindowId, CommandUILocation.ContextMenu, paths);
      AfterCommand();
    }

    /// <summary>
    /// Mark unfixed files by removing a dot charcter "•" to the end of file name and
    /// update total number of unfixed source files and headers
    /// </summary>
    /// <param name="checkedFiles"></param>
    public void MarkUnfixedFiles(List<FileModel> checkedFiles)
    {
      foreach (var file in checkedFiles)
      {
        MarkUnfixedFiles(file);
      }
    }

    #endregion

    #region Private Method

    private bool CheckIsHeader(string fullFilePath)
    {
      FileInfo path = new FileInfo(fullFilePath);
      return path.FullName.Contains(".h") || path.FullName.Contains(".hpp") || path.FullName.Contains(".hh") || path.FullName.Contains(".hxx");
    }

    /// <summary>
    /// Check all files
    /// </summary>
    private void CheckAll()
    {
      foreach (var file in files)
      {
        file.IsChecked = true;
        UpdateCheckedNumber(file);
      }
      tidyToolWindowModel.IsChecked = true;
      tidyToolWindowModel.CountFilesModel.UpdateTotalChecked(files);
    }

    private void UncheckAll()
    {
      foreach (var file in files)
      {
        file.IsChecked = false;
        UpdateCheckedNumber(file);
      }
      tidyToolWindowModel.IsChecked = false;
      tidyToolWindowModel.CountFilesModel.UpdateToUncheckAll();
    }

    private void MarkUnfixedFiles(FileModel file)
    {
      if (file.IsFixed)
      {
        file.IsFixed = false;
        file.FileName = file.FileName.Remove(file.FileName.Length - 2, 2);
        tidyToolWindowModel.CountFilesModel.UpdateFixFileState(file);
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
    /// Refresh values after tidy from toolbar or contextMenu
    /// </summary>
    private void RefreshValues()
    {
      files.Clear();
      headers.Clear();
      tidyToolWindowModel.IsChecked = false;
      tidyToolWindowModel.CountFilesModel.UpdateToUncheckAll();
    }
  }
  
  #endregion

}
