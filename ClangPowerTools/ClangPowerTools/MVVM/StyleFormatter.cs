using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ClangPowerTools
{
  public class StyleFormatter
  {
    #region Members

    private readonly SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();

    #endregion


    #region Methods

    public string FormatText(string textToFormat, List<IFormatOption> formatStyleOptions, EditorStyles selectedStyle)
    {
      CreatePaths(out string filePath, out string formatFilePath, out string folderPath);

      FileSystem.WriteContentToFile(formatFilePath, FormatOptionFile.CreateOutput(formatStyleOptions, selectedStyle).ToString());
      FileSystem.WriteContentToFile(filePath, textToFormat);

      var content = FormatFileOutsideProject(settingsPathBuilder.GetPath(""), filePath);

      FileSystem.DeleteDirectory(folderPath);
      return content;
    }


    private void CreatePaths(out string filePath, out string formatFilePath, out string folderPath)
    {
      string parentFolder = Path.Combine(settingsPathBuilder.GetPath(""), "Format");
      FileSystem.CreateDirectory(parentFolder);

      folderPath = Path.Combine(parentFolder, (Directory.GetDirectories(parentFolder).Length + 1).ToString());
      FileSystem.CreateDirectory(folderPath);

      filePath = Path.Combine(folderPath.ToString(), "FormatTemp.cpp");
      formatFilePath = Path.Combine(settingsPathBuilder.GetPath(""), folderPath.ToString(), ".clang-format");
    }

    private string FormatFileOutsideProject(string directoryPath, string filePath)
    {
      string vsixPath = Path.GetDirectoryName(typeof(RunClangPowerToolsPackage).Assembly.Location);
      string output = string.Empty;

      if (String.IsNullOrWhiteSpace(vsixPath) || String.IsNullOrWhiteSpace(directoryPath)
        || String.IsNullOrWhiteSpace(filePath))
      {
        return string.Empty;
      }

      try
      {
        var process = new Process();
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.FileName = Path.Combine(vsixPath, ScriptConstants.kClangFormat);
        process.StartInfo.WorkingDirectory = directoryPath;
        process.StartInfo.Arguments = $"-style=file \"{Path.GetFullPath(filePath)}\"";

        process.Start();
        output = process.StandardOutput.ReadToEnd();
        if (string.IsNullOrWhiteSpace(output))
        {
          output = process.StandardError.ReadToEnd();
        }
        process.WaitForExit();
        process.Close();
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Clang-Format Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }

      return output;
    }
    #endregion
  }
}
