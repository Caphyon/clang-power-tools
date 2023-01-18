using ClangPowerTools.Helpers;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ClangPowerTools
{
  public class FormatEditorController
  {
    #region Members

    public static Process EditorProcess;

    private static string vsixPath;

    #endregion 

    #region Constructor

    public FormatEditorController()
    {
      vsixPath = Path.GetDirectoryName(GetType().Assembly.Location);
      CreateEditorDirectory();
    }

    #endregion

    #region Methods

    public void OpenEditor()
    {
      try
      {
        EditorProcess = new Process();
        EditorProcess.StartInfo.FileName = Path.Combine(FormatEditorConstants.ClangFormatEditorPath,
        FormatEditorConstants.ExecutableName);
        EditorProcess.Start();
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, FormatEditorConstants.ClangFormatEditor, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    //Install Clang Format Editor if this is not already installed
    public void InstallClangFormatEditorSilent()
    {
      if (File.Exists(Path.Combine(FormatEditorConstants.ClangFormatEditorPath,
        FormatEditorConstants.ExecutableName)))
        return;
      try
      {
        var msiName = Path.Combine(vsixPath, FormatEditorConstants.ClangFormatMsi);
        string Script = $"-ExecutionPolicy Unrestricted -NoProfile -Noninteractive -command \"& " +
          $"msiexec.exe /i '{Path.Combine(vsixPath, FormatEditorConstants.ClangFormatMsi)}' /qn \" ";
        EditorProcess = new Process();
        EditorProcess.StartInfo.FileName = $"{Environment.SystemDirectory}\\{ScriptConstants.kPowerShellPath}";
        EditorProcess.StartInfo.Arguments = Script;
        EditorProcess.StartInfo.UseShellExecute = true;
        EditorProcess.StartInfo.Verb = "runas";
        EditorProcess.Start();
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, FormatEditorConstants.ClangFormatEditor, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private static void CreateEditorDirectory()
    {
      var appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FormatEditorConstants.ClangFormatEditorFolder);
      try
      {
        FileSystem.CreateDirectory(appDataDirectory);
        File.Copy(Path.Combine(vsixPath, FormatEditorConstants.ClangFormatExe),
                  Path.Combine(appDataDirectory, FormatEditorConstants.ClangFormatExe), true);
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, FormatEditorConstants.SetupFailed, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    #endregion
  }
}
