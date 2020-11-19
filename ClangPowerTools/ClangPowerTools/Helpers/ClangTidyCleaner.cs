using System.Collections.Generic;
using System.IO;

namespace ClangPowerTools.Helpers
{
  public class ClangTidyCleaner
  {
    #region Members

    private const string CLANG_TIDY_FILE = ".clang-tidy";
    private const string CLANG_TIDY_BACKUP_FILE = ".clang-tidy.cpt_backup";

    #endregion


    #region Methods

    public void Remove(string directoryPath)
    {
      var clangTidyFilePath = Path.Combine(directoryPath, CLANG_TIDY_FILE);
      var clangTidyBackupFilePath = Path.Combine(directoryPath, CLANG_TIDY_BACKUP_FILE);

      var settingsPathBuilder = new SettingsPathBuilder();
      var settingsPath = settingsPathBuilder.GetPath("");
      var tempClangTidyFilePath = Path.Combine(settingsPath, CLANG_TIDY_FILE);

      // if in the same directory already exists a .clang-tidy file
      if (File.Exists(tempClangTidyFilePath))
      {
        // if the backup file from script still exists
        if (File.Exists(clangTidyBackupFilePath))
        {
          File.Copy(clangTidyBackupFilePath, clangTidyFilePath, true);
          File.Delete(clangTidyBackupFilePath);
        }
        else // backup file from script was deleted but we still have the backup file from %appdata%
        {
          File.Copy(tempClangTidyFilePath, clangTidyFilePath, true);
        }
        File.Delete(tempClangTidyFilePath);
        return;
      }

      // the .clang-tidy file was created by the script
      // if it wasn't deteled by the script then delete it now
      if (File.Exists(clangTidyFilePath))
        File.Delete(clangTidyFilePath);
    }

    public void Remove(List<string> mDirectoriesPath)
    {
      foreach (var directoryPath in mDirectoriesPath)
        Remove(directoryPath);
    }

    #endregion

  }
}
