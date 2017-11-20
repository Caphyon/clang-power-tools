using System.Collections.Generic;
using System.IO;

namespace ClangPowerTools
{
  public class PCHCleaner
  {
    #region Public Methods

    public void Clean(string aDirectoriesPath)
    {
      var directories = Directory.GetDirectories(aDirectoriesPath);
      foreach (var directory in directories)
        Clean(directory);

      var files = Directory.GetFiles(aDirectoriesPath, $"*{ScriptConstants.kTempFileExtension}");
      RemoveFiles(files);
    }

    public void Clean(IEnumerable<string> aDirectoriesPath)
    {
      foreach (var directory in aDirectoriesPath)
      {
        var directories = Directory.GetDirectories(directory);
        Clean(directories);

        var files = Directory.GetFiles(directory, ScriptConstants.kTempFileExtension);
        RemoveFiles(files);
      }
    }

    #endregion

    #region Private Methods

    private void RemoveFiles(IEnumerable<string> aFiles)
    {
      foreach (var file in aFiles)
        File.Delete(file);
    }
    
    #endregion


  }
}
