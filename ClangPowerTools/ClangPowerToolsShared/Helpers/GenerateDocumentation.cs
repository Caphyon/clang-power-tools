using ClangPowerTools;
using ClangPowerToolsShared.MVVM.Constants;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ClangPowerToolsShared.Helpers
{
  public static class GenerateDocumentation
  {
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
    public static async Task<Process> CreateProcessGenerateDocumentationAsync(int commandId, bool jsonCompilationDbActive,
      List<string> paths)
    {
      GetClangDoc();

      string projectPath = string.Empty;
      if (paths.Any())
      {
        FileInfo fileInfo = new FileInfo(paths.FirstOrDefault());
        if (fileInfo.FullName.Contains(".sln"))
        {
          projectPath = fileInfo.Directory.FullName;
        }
        else
        {
          projectPath = fileInfo.Directory.Parent.FullName;
        }

        string jsonCompilationDatabasePath = Path.Combine(projectPath, ScriptConstants.kCompilationDBFile);
        string documentationOutoutePath = Path.Combine(projectPath, "Documentation");
        string clangDocPath = Path.Combine(PathConstants.LlvmLitePath, ScriptConstants.kClangDoc);

        if (File.Exists(jsonCompilationDatabasePath) && File.Exists(clangDocPath))
        {
          Process process = new Process();
          process.StartInfo.UseShellExecute = false;
          process.StartInfo.CreateNoWindow = true;
          process.StartInfo.RedirectStandardInput = true;
          process.StartInfo.RedirectStandardOutput = true;
          process.StartInfo.RedirectStandardError = true;
          process.StartInfo.FileName = $"{Environment.SystemDirectory}\\{ScriptConstants.kPowerShellPath}";
          process.StartInfo.Arguments = $"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -command '& " +
          $" ''{clangDocPath}'' --format="+ formats[commandId] + "-output=''{documentationOutoutePath}'' ''{jsonCompilationDatabasePath}'' '";

          return process;
        }
      }
      return new Process();
    }



    /// <summary>
    /// Run a process that download clang-doc.exe if it wasn't found on disk
    /// </summary>
    /// <exception cref="Exception"></exception>
    private static void GetClangDoc()
    {
      var getllvmPath = GetScriptFilePath();

      Process process = new Process();
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.RedirectStandardInput = true;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.FileName = $"{Environment.SystemDirectory}\\{ScriptConstants.kPowerShellPath}";
      process.StartInfo.Arguments = $"PowerShell.exe -ExecutionPolicy Unrestricted -NoProfile -Noninteractive -command '& " +
        $" ''{getllvmPath}'' {ScriptConstants.kClangDoc} '";

      try
      {
        process.Start();
      }
      catch (Exception exception)
      {
        throw new Exception(
            $"Cannot execute {process.StartInfo.FileName}.\n{exception.Message}.");
      }
    }

    private static string GetScriptFilePath()
    {
      var assemblyPath = Assembly.GetExecutingAssembly().Location;
      var scriptDirectory = assemblyPath.Substring(0, assemblyPath.LastIndexOf('\\'));

      return Path.Combine(scriptDirectory, "Tooling\\v1\\psClang", ScriptConstants.kGetLLVMScriptName);
    }
  }


}
