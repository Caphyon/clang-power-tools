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
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerToolsShared.MVVM.ViewModels.ToolWindow
{
  public class TidyToolWindowController
  {
    public ObservableCollection<FileModel> files = new ObservableCollection<FileModel>();
    private List<FileModel> headers = new List<FileModel>();
    public TidyToolWindowModel tidyToolWindowModel = new TidyToolWindowModel();


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
          var currentHeader = headers.Where(a => a.FullFileName == path.FullName).FirstOrDefault();
          if (currentHeader != null)
          {
            currentHeader.IsChecked = true;
            tidyToolWindowModel.CountFilesModel.CheckFileUpdate(currentHeader);

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
    /// Applies tidy fix on all selected source files or just on single file and makes diff after
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public async Task FixAllFilesAsync(FileModel file = null)
    {
      //TODO apply tidy fix just on source files, ignore headers
      BeforeCommand();
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
      AfterCommand();
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
        //Check "global" checkbox if all files are checked
        tidyToolWindowModel.CountFilesModel.CheckFileUpdate(file);
        tidyToolWindowModel.IsChecked = tidyToolWindowModel.CountFilesModel.TotalCheckedFiles == files.Count ? true : false;
      }
      else
      {
        //Uncheck "global" checkbox if no file is checked
        tidyToolWindowModel.CountFilesModel.UnCheckFileUpdate(file);
        tidyToolWindowModel.IsChecked = tidyToolWindowModel.CountFilesModel.TotalCheckedFiles == 0 || tidyToolWindowModel.CountFilesModel.TotalCheckedFiles != files.Count ? false : true;
      }
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

    /// <summary>
    /// Check all files
    /// </summary>
    private void CheckAll()
    {
      foreach (var file in files)
      {
        file.IsChecked = true;
        tidyToolWindowModel.CountFilesModel.CheckFileUpdate(file);
      }
      tidyToolWindowModel.IsChecked = true;
      tidyToolWindowModel.CountFilesModel.UpdateTotalChecked(files);
    }

    private void UncheckAll()
    {
      foreach (var file in files)
      {
        file.IsChecked = false;
        tidyToolWindowModel.CountFilesModel.UnCheckFileUpdate(file);
      }
      tidyToolWindowModel.IsChecked = false;
      tidyToolWindowModel.CountFilesModel.UpdateToUncheckAll();
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

    private void MarkUnfixedFiles(FileModel file)
    {
      if (file.IsFixed)
      {
        file.IsFixed = false;
        file.FileName = file.FileName.Remove(file.FileName.Length - 2, 2);
        tidyToolWindowModel.CountFilesModel.UpdateFixFileState(file);
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
        if(removeFile is not null)
        {
          tidyToolWindowModel.CountFilesModel.UnCheckFileUpdate(removeFile);
          files.Remove(removeFile);
        }
      }
      else
      {
        foreach (var file in files.ToList())
        {
          if (file.IsChecked)
          {
            tidyToolWindowModel.CountFilesModel.UnCheckFileUpdate(file);
            files.Remove(file);
          }
        }
      }
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
    /// Refresh values after tidy from toolbar or contextMenu
    /// </summary>
    public void RefreshValues()
    {
      files.Clear();
      headers.Clear();
      tidyToolWindowModel.IsChecked = false;
      tidyToolWindowModel.CountFilesModel.UpdateToUncheckAll();
    }

    #endregion

  }
}
