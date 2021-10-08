using ClangPowerTools;
using ClangPowerTools.Services;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClangPowerToolsShared.MVVM.Commands
{
  public class TidyDiffCommand
  {

    public async Task TidyDiffAsync(List<string> filesPath)
    {

      var clangTidyPath = Path.Combine(ClangPowerTools.SettingsProvider.LlvmSettingsModel.PreinstalledLlvmPath, "clang-tidy.exe");
      if (filesPath.Count == 1)
      {
        try
        {
          FileInfo file = new(filesPath.First());
          var copyFile = Path.Combine(file.Directory.FullName, "_" + file.Name);
          File.Copy(file.FullName, copyFile, true);
          System.Diagnostics.Process process = new();
          process.StartInfo.FileName = clangTidyPath;
          process.StartInfo.CreateNoWindow = true;
          process.StartInfo.UseShellExecute = false;
          process.StartInfo.Arguments = $"-fix \"{copyFile}\"";
          process.Start();
          process.WaitForExit();
          DiffFilesUsingDefaultTool(copyFile, file.FullName);
          File.Delete(copyFile);
        }
        catch (Exception e)
        {
          MessageBox.Show(e.Message, "Tidy-Diff Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
    }

    public void TidyDiff(string filePath)
    {

      var clangTidyPath = Path.Combine(ClangPowerTools.SettingsProvider.LlvmSettingsModel.PreinstalledLlvmPath, "clang-tidy.exe");

      try
      {
        FileInfo file = new(filePath);
        var copyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClangPowerTools", "Temp", file.Name);
        File.Copy(file.FullName, copyFile, true);
        System.Diagnostics.Process process = new();
        process.StartInfo.FileName = clangTidyPath;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.Arguments = $"-fix \"{file.FullName}\"";
        process.Start();
        process.WaitForExit();
        DiffFilesUsingDefaultTool(copyFile, file.FullName);
        //File.Delete(copyFile);
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Tidy-Diff Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }

    }

    public static void DiffFilesUsingDefaultTool(string file1, string file2)
    {
      object args = $"\"{file1}\" \"{file2}\"";
      var dte = VsServiceProvider.GetService(typeof(DTE2)) as DTE2;
      dte.Commands.Raise(TidyConstants.ToolsDiffFilesCmd, TidyConstants.ToolsDiffFilesId, ref args, ref args);
    }
  }
}
