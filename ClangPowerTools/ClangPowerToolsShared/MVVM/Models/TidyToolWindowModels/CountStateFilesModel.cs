using ClangPowerTools.MVVM.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models.TidyToolWindowModels
{
  /// <summary>
  /// This class can be used to store all data about files state
  /// </summary>
  public class CountFilesModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private int totalCheckedFixedFiles = 0;
    private int totalCheckedFixedHeaders = 0;
    private int totalCheckedFixedSouceFiles = 0;
    private int totalCheckedSourceFiles = 0;
    private int totalCheckedHeaders = 0;
    private int totalCheckedFiles = 0;

    /// <summary>
    /// Returns number of checked fixed files at this moment
    /// </summary>
    public int TotalCheckedFixedFiles
    {
      get { return totalCheckedFixedFiles; }
      set
      {
        totalCheckedFixedFiles = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalCheckedFixedFiles"));
      }
    }

    /// <summary>
    /// Returns number of checked fixed headers at this moment
    /// </summary>
    public int TotalCheckedFixedHeaders
    {
      get
      { return totalCheckedFixedHeaders; }
      set
      {
        totalCheckedFixedHeaders = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalCheckedFixedHeaders"));
      }
    }

    /// <summary>
    /// Returns number of checked fixed source files at this moment
    /// </summary>
    public int TotalCheckedFixedSouceFiles
    {
      get { return totalCheckedFixedSouceFiles; }
      set
      {
        totalCheckedFixedSouceFiles = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalCheckedFixedSouceFiles"));
      }
    }

    /// <summary>
    /// Returns number of checked source files at this moment
    /// </summary>
    public int TotalCheckedSourceFiles
    {
      get { return totalCheckedSourceFiles; }
      set
      {
        totalCheckedSourceFiles = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalCheckedSourceFiles"));
      }
    }

    /// <summary>
    /// Returns number of checked headers at this moment
    /// </summary>
    public int TotalCheckedHeaders
    {
      get { return totalCheckedHeaders; }
      set
      {
        totalCheckedHeaders = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalCheckedHeaders"));
      }
    }

    /// <summary>
    /// Returns number of checked files at this moment
    /// </summary>
    public int TotalCheckedFiles
    {
      get { return totalCheckedFiles; }
      set
      {
        totalCheckedFiles = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalCheckedFiles"));
      }
    }


    /// <summary>
    /// Total checked property will be updated with number of files
    /// </summary>
    /// <param name="files"></param>
    public void UpdateTotalChecked(ObservableCollection<FileModel> files)
    {
      TotalCheckedFiles = files.Count;
    }

    /// <summary>
    /// Update properties refering on check file 
    /// </summary>
    public void CheckFileUpdate(FileModel file)
    {
      if (file is not null && file.IsChecked)
      {
        ++TotalCheckedFiles;
        if (file.FilesType == FileType.SourceFile)
          ++TotalCheckedSourceFiles;
        if (file.FilesType == FileType.Header)
          ++TotalCheckedHeaders;

        //Update values for fixed files
        if (file.IsFixed)
        {
          ++TotalCheckedFixedFiles;
          if (file.FilesType == FileType.SourceFile)
          {
            ++TotalCheckedFixedSouceFiles;
          }
          if (file.FilesType == FileType.Header)
          {
            ++TotalCheckedFixedHeaders;
          }
        }
      }
    }

    /// <summary>
    /// Update properties refering on uncheck file 
    /// </summary>
    public void UnCheckFileUpdate(FileModel file)
    {
      if (file is not null && !file.IsChecked)
      {
        --TotalCheckedFiles;
        if (file.FilesType == FileType.SourceFile)
          --TotalCheckedSourceFiles;
        if (file.FilesType == FileType.Header)
          --TotalCheckedHeaders;
      }

      //Update values for fixed files
      if (file.IsFixed)
      {
        --TotalCheckedFixedFiles;
        if (file.FilesType == FileType.SourceFile)
        {
          --TotalCheckedFixedSouceFiles;
        }
        if (file.FilesType == FileType.Header)
        {
          --TotalCheckedFixedHeaders;
        }
      }
    }

    /// <summary>
    /// Update all properties to 0
    /// </summary>
    public void UpdateToUncheckAll()
    {
      TotalCheckedFixedFiles = 0;
      TotalCheckedFixedHeaders = 0;
      TotalCheckedFixedSouceFiles = 0;
      TotalCheckedSourceFiles = 0;
      TotalCheckedHeaders = 0;
      TotalCheckedFiles = 0;
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
          ++TotalCheckedFixedFiles;
          if (file.FilesType == FileType.SourceFile)
          {
            ++TotalCheckedFixedSouceFiles;
          }
          if (file.FilesType == FileType.Header)
          {
            ++TotalCheckedFixedHeaders;
          }
        }
        else
        {
          //Update just unfixed properties
          --TotalCheckedFixedFiles;
          if (file.FilesType == FileType.SourceFile)
          {
            --TotalCheckedFixedSouceFiles;
          }
          if (file.FilesType == FileType.Header)
          {
            --TotalCheckedFixedHeaders;
          }
        }
      }
    }

  }
}
