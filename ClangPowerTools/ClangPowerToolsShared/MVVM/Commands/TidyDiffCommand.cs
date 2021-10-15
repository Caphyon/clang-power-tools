using ClangPowerTools;
using ClangPowerTools.Services;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace ClangPowerToolsShared.MVVM.Commands
{
  public static class TidyDiffCommand
  {
    public static readonly string tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClangPowerTools", "Temp");
    public static void TidyFixDiff(string filePath, bool makeDiff = true)
    {

      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      var clangTidyPath = settingsPathBuilder.GetCurrentExecutableLlvmPath();
      try
      {
        FileInfo file = new(filePath);
        var copyFile = Path.Combine(tempPath, file.Name);
        File.Copy(file.FullName, copyFile, true);
        System.Diagnostics.Process process = new();
        process.StartInfo.FileName = clangTidyPath;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.Arguments = $"-fix \"{file.FullName}\"";
        process.Start();
        process.WaitForExit();
        if (makeDiff)
          DiffFilesUsingDefaultTool(copyFile, file.FullName);
        //File.Delete(copyFile);
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Tidy-Diff Failed - you do not have an llvm set", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }

    }

    public static void CopyFileInTemp(string filePath)
    {
      FileInfo file = new(filePath);
      var copyFile = Path.Combine(tempPath, file.Name);
      File.Copy(file.FullName, copyFile, true);
    }

    public static void CopyFilesInTemp(List<string> filePaths)
    {
      foreach(var path in filePaths)
      {
        FileInfo file = new(path);
        var copyFile = Path.Combine(tempPath, file.Name);
        File.Copy(file.FullName, copyFile, true);
      }
    }

    public static string CreateTempFilePath(string path)
    {
      FileInfo fileInfo = new(path);
      return Path.Combine(tempPath, fileInfo.Name);
    }

    public static void DiffFilesUsingDefaultTool(string file1, string file2)
    {
      object args = $"\"{file1}\" \"{file2}\"";
      var dte = VsServiceProvider.GetService(typeof(DTE2)) as DTE2;
      dte.Commands.Raise(TidyConstants.ToolsDiffFilesCmd, TidyConstants.ToolsDiffFilesId, ref args, ref args);
    }
  }
}
