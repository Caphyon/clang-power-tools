using System.IO;

namespace ClangPowerTools
{
  public class FileChangerWatcher
  {
    #region Members

    private static FileSystemWatcher mWatcher = new FileSystemWatcher();

    #endregion

    #region Properties

    public static FileSystemEventHandler OnChanged { get; set; }

    #endregion

    #region Public methods

    public static void Run(string aDirectoryPath)
    {
      if (null == aDirectoryPath || string.IsNullOrWhiteSpace(aDirectoryPath))
        return;

      // Set the path property of FileSystemWatcher
      mWatcher.Path = aDirectoryPath;

      // Watch for changes in LastWrite time
      mWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
           | NotifyFilters.FileName | NotifyFilters.DirectoryName;

      // Watch all accepted files 
      foreach (var extension in ScriptConstants.kAcceptedFileExtensions)
        mWatcher.Filter = $"*{extension}";

      //Subdirectories will be also watched.
      mWatcher.IncludeSubdirectories = true;

      // Watch every file in the directory for changes
      mWatcher.Changed += OnChanged;
      mWatcher.Deleted += OnChanged;

      // Begin watching.
      mWatcher.EnableRaisingEvents = true;
    }

    #endregion

  }
}
