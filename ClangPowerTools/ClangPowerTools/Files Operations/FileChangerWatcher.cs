using System.Collections.Generic;
using System.IO;

namespace ClangPowerTools
{
  public class FileChangerWatcher
  {
    #region Members

    private static List<FileSystemWatcher> mWatchers = new List<FileSystemWatcher>();

    #endregion


    #region Properties

    public static FileSystemEventHandler OnChanged { get; set; }

    #endregion


    #region Public methods

    public static void Run(string aDirectoryPath)
    {
      if (null == aDirectoryPath || string.IsNullOrWhiteSpace(aDirectoryPath))
        return;

      foreach (var extension in ScriptConstants.kAcceptedFileExtensions)
        mWatchers.Add(CreateFileWatcher(aDirectoryPath, extension));
    }


    #endregion


    #region Private Methods


    private static FileSystemWatcher CreateFileWatcher(string aDirectoryPath, string aExtension)
    {
      FileSystemWatcher fileWatcher = new FileSystemWatcher();

      // Set the path property of FileSystemWatcher
      fileWatcher.Path = aDirectoryPath;

      // Watch for changes in LastWrite time
      fileWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
           | NotifyFilters.FileName | NotifyFilters.DirectoryName;

      // Watch files with specific file extension
      fileWatcher.Filter = $"*{aExtension}";

      //Subdirectories will be also watched.
      fileWatcher.IncludeSubdirectories = true;

      // Watch every file in the directory for changes
      fileWatcher.Changed += OnChanged;
      fileWatcher.Deleted += OnChanged;

      // Begin watching.
      fileWatcher.EnableRaisingEvents = true;

      return fileWatcher;
    }

    #endregion

  }
}
