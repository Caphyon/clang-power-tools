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
      {
        FileSystemWatcher newFileWatcher = new FileSystemWatcher();

        // Set the path property of FileSystemWatcher
        newFileWatcher.Path = aDirectoryPath;

        // Watch for changes in LastWrite time
        newFileWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
             | NotifyFilters.FileName | NotifyFilters.DirectoryName;

        // Watch files with specific file extension
        newFileWatcher.Filter = $"*{extension}";

        //Subdirectories will be also watched.
        newFileWatcher.IncludeSubdirectories = true;

        // Watch every file in the directory for changes
        newFileWatcher.Changed += OnChanged;
        newFileWatcher.Deleted += OnChanged;

        // Begin watching.
        newFileWatcher.EnableRaisingEvents = true;

        // Save the watcher
        mWatchers.Add(newFileWatcher);
      }
    }


    #endregion

  }
}
