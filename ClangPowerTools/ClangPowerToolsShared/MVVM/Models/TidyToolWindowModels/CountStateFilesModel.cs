using ClangPowerTools.MVVM.Models;
using System.Collections.ObjectModel;

namespace ClangPowerToolsShared.MVVM.Models.TidyToolWindowModels
{
  public class CountFilesModel
  {
    private int totalCheckedFixedFiles = 0;
    private int totalCheckedFiles = 0;
    private int totalCheckedHeaders = 0;
    private int totalChecked = 0;

    /// <summary>
    /// Returns number of checked fixed files at this moment
    /// </summary>
    public int TotalCheckedFixedFiles { get { return totalCheckedFixedFiles;  } }

    /// <summary>
    /// Returns number of checked source files at this moment
    /// </summary>
    public int TotalCheckedFiles { get { return totalCheckedFiles; } }

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
  }
}
