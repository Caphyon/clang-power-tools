using ClangPowerTools;
using ClangPowerTools.Commands;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerToolsShared.Helpers
{
  public static class GenerateDocumentation
  {
    public static string OutputDir { get; set; } = string.Empty;
    public static EventHandler ExitedHandler { get; set; }

    public static Dictionary<int, string> Formats =
      new Dictionary<int, string>()
      {
        {534, "yaml" },
        {278, "html" },
        {294, "md" }
      };


    /// <summary>
    /// Find a name for output documentation folder
    /// </summary>
    public static string FindOutputFolderName(string outputPath)
    {
      while (Directory.Exists(outputPath))
      {
        //Get number from folder path
        FileInfo fileInfo = new FileInfo(outputPath);
        var resultString = Regex.Match(fileInfo.Directory.Name, @"\d+").Value;
        if (resultString != string.Empty)
        {
          var resultNumber = Int32.Parse(resultString);
          //Increment number and replace in foder path
          ++resultNumber;
          var resultFolderName = fileInfo.Directory.Name.Replace(resultString, resultNumber.ToString());
          outputPath = outputPath.Replace(fileInfo.Directory.Name, resultFolderName);
        }
        else
        {
          //Start folder count from 1, by adding 1 to final
          var resultFolderName = fileInfo.Directory.Name + " (1)";
          outputPath = outputPath.Replace(fileInfo.Directory.Name, resultFolderName);
        }
      }
      //Delete last two charachers (\\) from end
      return outputPath.Remove(outputPath.Length - 1, 1);
    }

    public static void DisplayInfoMessage(string outputPath)
    {
      CommandControllerInstance.CommandController.DisplayMessage(false,
      $"Generated Documentation at path: {outputPath}");
    }

    /// <summary>
    /// Run a process that download clang-doc.exe (and returns path) if it wasn't found on disk
    /// </summary>
    /// <exception cref="Exception"></exception>
    public static string GetClangDoc()
    {
      var getllvmScriptPath = GetScriptFilePath();

      Process process = new Process();
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.RedirectStandardInput = true;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.EnvironmentVariables["Path"] = PowerShellWrapper.CreatePathEnvironmentVariable();
      process.StartInfo.FileName = $"{Environment.SystemDirectory}\\{ScriptConstants.kPowerShellPath}";
      process.StartInfo.Arguments = $"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -command '& " +
        $" ''{getllvmScriptPath}'' {ScriptConstants.kClangDoc} '";

      try
      {
        process.Start();
        while (!process.StandardOutput.EndOfStream)
        {
          return process.StandardOutput.ReadLine();
        }
      }
      catch (Exception exception)
      {
        throw new Exception(
            $"Cannot execute {process.StartInfo.FileName}.\n{exception.Message}.");
      }
      return string.Empty;
    }

    private static string GetScriptFilePath()
    {
      var assemblyPath = Assembly.GetExecutingAssembly().Location;
      var scriptDirectory = assemblyPath.Substring(0, assemblyPath.LastIndexOf('\\'));

      return Path.Combine(scriptDirectory, "Tooling\\v1\\psClang", ScriptConstants.kGetLLVMScriptName);
    }


    public static void ClosedDataConnection(object sender, EventArgs e)
    {
      int id = CommandControllerInstance.CommandController.GetCommandIdForGenerateDoc();
      if (Formats.ContainsKey(id))
      {
        OpenInFileExplorer(OutputDir);
        DisplayInfoMessage(OutputDir);
      }
    }

    public static void OpenInFileExplorer(string path)
    {
      if (!Directory.Exists(path))
        return;

      // combine the arguments together
      // it doesn't matter if there is a space after ','
      string argument = " \"" + path + "\"";

      // open the file in File Explorer and select it
      Process.Start("explorer.exe", argument);
    }
  }


}
