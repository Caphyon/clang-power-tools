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
    private int totalCheckedFiles = 0;
    private int totalCheckedHeaders = 0;
    private int totalChecked = 0;

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
    public int TotalCheckedSourceFiles { get { return totalCheckedFiles; } }

    /// <summary>
    /// Returns number of checked headers at this moment
    /// </summary>
    public int TotalCheckedHeaders { get { return totalCheckedHeaders; } }

    /// <summary>
    /// Returns number of checked files at this moment
    /// </summary>
    public int TotalChecked { get { return totalChecked; } }


    /// <summary>
    /// Given a collection of files all CountFile properies will be updated. 
    /// </summary>
    /// <param name="files"></param>
    public void UpdateTotalChecked(ObservableCollection<FileModel> files)
    {
      totalChecked = files.Count;
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
      totalChecked = 0;
    }

    /// <summary>
    /// Update fixed number for source file and headers
    /// </summary>
    public void UpdateFix(FileModel file)
    {
      ++totalCheckedFixedFiles;
      if (file.FilesType == FileType.SourceFile)
      {
        ++totalCheckedFixedSouceFiles;
      }
      if(file.FilesType == FileType.Header)
      {
        ++totalCheckedFixedHeaders;
      }
    }

    /// <summary>
    /// Update unfixed number for source file and headers
    /// </summary>
    /// <param name="file"></param>
    public void UpdateUnfix(FileModel file)
    {
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
