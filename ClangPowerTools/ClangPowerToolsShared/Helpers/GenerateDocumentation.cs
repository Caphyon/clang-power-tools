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
    private static string OutputDir { get; set; } = string.Empty;
    public static EventHandler ExitedHandler { get; set; }

    private static Dictionary<int, string> formats =
      new Dictionary<int, string>()
      {
        {534, "yaml" },
        {278, "html" },
        {294, "md" }
      };

    /// <summary>
    /// Create a process for running clang-doc.exe resulted
    /// format, depends on passed command
    /// </summary>
    /// <param name="commandId"></param>
    /// <param name="jsonCompilationDbActive"></param>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static async Task GenerateDocumentationForProjectAsync(int commandId,
      bool jsonCompilationDbActive, AsyncPackage package)
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
      var jsonCompilationDatabasePath = Path.Combine(JsonCompilationDatabaseCommand.Instance.SolutionPath(),
        ScriptConstants.kCompilationDBFile);
      string documentationOutoutePath = FindOutputFolderName(Path.Combine(JsonCompilationDatabaseCommand.Instance.SolutionPath(),
        "Documentation\\"));

      string clangDocPath = GetClangDoc();
      clangDocPath = Path.Combine(clangDocPath, ScriptConstants.kClangDoc);

      if (File.Exists(jsonCompilationDatabasePath) && File.Exists(clangDocPath))
      {
        Process process = new Process();
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        process.EnableRaisingEvents = true;
        process.Exited += ExitedHandler;
        process.Disposed += ExitedHandler;

        process.StartInfo.FileName = $"{Environment.SystemDirectory}\\{ScriptConstants.kPowerShellPath}";
        process.StartInfo.Arguments = $"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -command '& " +
        $" ''{clangDocPath}'' --format={formats[commandId]}  -output=''{documentationOutoutePath}'' ''{jsonCompilationDatabasePath}'' '";
        try
        {
          process.Start();
          OutputDir = documentationOutoutePath;
        }
        catch (Exception exception)
        {
          process.EnableRaisingEvents = false;
          process.Exited -= ExitedHandler;
          process.Disposed -= ExitedHandler;

          throw new Exception(
              $"Cannot execute {process.StartInfo.FileName}.\n{exception.Message}.");
        }
      }
    }


    /// <summary>
    /// Find a name for output documentation folder
    /// </summary>
    private static string FindOutputFolderName(string outputPath)
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

    private static void DisplayInfoMessage(string outputPath)
    {
      CommandControllerInstance.CommandController.DisplayMessage(false,
      $"Generated Documentation at path: {outputPath}");
    }

    /// <summary>
    /// Run a process that download clang-doc.exe (and returns path) if it wasn't found on disk
    /// </summary>
    /// <exception cref="Exception"></exception>
    private static string GetClangDoc()
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
      DisplayInfoMessage(OutputDir);
      OpenInFileExplorer(OutputDir);
    }

    private static void OpenInFileExplorer(string path)
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
