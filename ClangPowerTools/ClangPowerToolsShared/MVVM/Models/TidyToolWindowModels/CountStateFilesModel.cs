using ClangPowerTools.MVVM.Models;
using System.Collections.ObjectModel;

namespace ClangPowerToolsShared.MVVM.Models.TidyToolWindowModels
{
  /// <summary>
  /// This class can be used to store all data about files state
  /// </summary>
  public class CountFilesModel
  {
    private int totalCheckedFixedFiles = 0;
    private int totalCheckedFixedHeaders = 0;
    private int totalCheckedFixedSouceFiles = 0;
    private int totalCheckedSourceFiles = 0;
    private int totalCheckedHeaders = 0;
    private int totalCheckedFiles = 0;

    /// <summary>
    /// Returns number of checked fixed files at this moment
    /// </summary>
    public int TotalCheckedFixedFiles { get { return totalCheckedFixedFiles; } }

    /// <summary>
    /// Returns number of checked fixed headers at this moment
    /// </summary>
    public int TotalCheckedFixedHeaders { get { return totalCheckedFixedHeaders; } }

    /// <summary>
    /// Returns number of checked fixed source files at this moment
    /// </summary>
    public int TotalCheckedFixedSouceFiles { get { return totalCheckedFixedSouceFiles; } }

    /// <summary>
    /// Returns number of checked source files at this moment
    /// </summary>
    public int TotalCheckedSourceFiles { get { return totalCheckedSourceFiles; } }

    /// <summary>
    /// Returns number of checked headers at this moment
    /// </summary>
    public int TotalCheckedHeaders { get { return totalCheckedHeaders; } }

    /// <summary>
    /// Returns number of checked files at this moment
    /// </summary>
    public int TotalCheckedFiles { get { return totalCheckedFiles; } }


    /// <summary>
    /// Total checked property will be updated with number of files
    /// </summary>
    /// <param name="files"></param>
    public void UpdateTotalChecked(ObservableCollection<FileModel> files)
    {
      totalCheckedFiles = files.Count;
    }

    /// <summary>
    /// Update properties refering on check file 
    /// </summary>
    public void CheckFileUpdate(FileModel file)
    {
      if (file is not null && file.IsChecked)
      {
        ++totalCheckedFiles;
        if (file.FilesType == FileType.SourceFile)
          ++totalCheckedSourceFiles;
        if (file.FilesType == FileType.Header)
          ++totalCheckedHeaders;
      }

    }

    /// <summary>
    /// Update properties refering on uncheck file 
    /// </summary>
    public void UnCheckFileUpdate(FileModel file)
    {
      if (file is not null && !file.IsChecked)
      {
        --totalCheckedFiles;
        if (file.FilesType == FileType.SourceFile)
          --totalCheckedSourceFiles;
        if (file.FilesType == FileType.Header)
          --totalCheckedHeaders;
      }
    }

    /// <summary>
    /// Update all properties to 0
    /// </summary>
    public void UpdateToUncheckAll()
    {
      totalCheckedFixedFiles = 0;
      totalCheckedFixedHeaders = 0;
      totalCheckedFixedSouceFiles = 0;
      totalCheckedFiles = 0;
      totalCheckedHeaders = 0;
      totalCheckedFiles = 0;
    }

    /// <summary>
    /// Update fixed and unfixed number for source file and headers.
    /// Make changes of peroperties based on fact that passed file is fixed or unfixed
    /// </summary>
    public void UpdateFixFileState(FileModel file)
    {
      if (file.IsChecked)
      {
        if (file.IsFixed)
        {
          //Update just fix properties
          ++totalCheckedFixedFiles;
          if (file.FilesType == FileType.SourceFile)
          {
            ++totalCheckedFixedSouceFiles;
          }
          if (file.FilesType == FileType.Header)
          {
            ++totalCheckedFixedHeaders;
          }
        }
        else
        {
          //Update just unfixed properties
          --totalCheckedFixedFiles;
          if (file.FilesType == FileType.SourceFile)
          {
            --totalCheckedFixedSouceFiles;
          }
          if (file.FilesType == FileType.Header)
          {
            --totalCheckedFixedHeaders;
          }
        }
      }
    }

  }
}
