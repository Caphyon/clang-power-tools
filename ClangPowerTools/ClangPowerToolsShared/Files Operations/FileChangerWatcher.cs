using System;
using System.Collections.Generic;
using System.IO;

namespace ClangPowerTools
{
  public class FileChangerWatcher : IDisposable
  {
    #region Members

    private List<FileSystemWatcher> mWatchers = new List<FileSystemWatcher>();

    #endregion


    #region Properties

    public FileSystemEventHandler OnChanged { get; set; }

    #endregion


    #region Public methods


    public void Run(string aDirectoryPath)
    {
      if (null == aDirectoryPath || string.IsNullOrWhiteSpace(aDirectoryPath))
        return;

      foreach (var extension in ScriptConstants.kAcceptedFileExtensions)
        mWatchers.Add(CreateFileWatcher(aDirectoryPath, extension));
    }


    #region IDisposable implementation


    public void Dispose()
    {
      OnChanged = null;
      mWatchers = null;
    }

    #endregion


    #endregion


    #region Private Methods


    private FileSystemWatcher CreateFileWatcher(string aDirectoryPath, string aExtension)
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
