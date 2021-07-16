using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace ClangPowerTools.Helpers
{
  public static class FormatEditorUtility
  {
    #region Methods

    public static void OpenBrowser()
    {
      try
      {
        Process.Start(FormatEditorConstants.FrameworkdUrlDownload);
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Clang-Format Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    public static bool FrameworkInstalled()
    {
      if (Directory.Exists(FormatEditorConstants.FrameworkPath))
      {
        var directories = Directory.GetDirectories(FormatEditorConstants.FrameworkPath)
                            .Select(Path.GetFileName)
                            .ToArray();
        foreach (var directory in directories)
        {
          if (directory.StartsWith("5.")) return true;
        }
      }

      return false;
    }

    #endregion

  }
}
