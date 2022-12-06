using ClangPowerTools.Output;
using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.MVVM.Constants;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Process = System.Diagnostics.Process;

namespace ClangPowerTools
{
  public static class PowerShellWrapper
  {
    #region Properties

    public static DataReceivedEventHandler DataErrorHandler { get; set; }
    public static DataReceivedEventHandler DataHandler { get; set; }
    public static EventHandler ExitedHandler { get; set; }

    public static OutputWindowController mOutputWindowController;
    public static string InteractivCommands { get; set; } = string.Empty;
    private static bool mInteractiveMode = false;
    private static Process mInteractiveProcess;


    #endregion

    #region Public Methods


    public static bool Invoke(string aScript, bool aUsePwshFileName = false)
    {
      if (SettingsProvider.CompilerSettingsModel.Powershell7
        && string.IsNullOrEmpty(GetFilePathFromEnviromentVar(ScriptConstants.kPwsh)))
      {
        SettingsHandler settingsHandler = new SettingsHandler();
        SettingsProvider.CompilerSettingsModel.Powershell7 = false;
        settingsHandler.SaveSettings();
        mOutputWindowController.Write("Can't find PowerShell 7 in PATH");
        return false;
      }

      Process process = new Process();
      try
      {

        process.StartInfo = new ProcessStartInfo()
        {
          FileName = $"{Environment.SystemDirectory}\\{ScriptConstants.kPowerShellPath}",
          RedirectStandardError = true,
          RedirectStandardOutput = true,
          CreateNoWindow = true,
          UseShellExecute = false,

          /*
          When we are dealing with file paths that contain single quotes, we are running into 
          trouble because whey are messing up our script invocation text. The situation is further 
          complicated by the fact that this invocation is imbricated (invoke inside invoke).
          Explanation: we are invoking powershell.exe and telling it using -command what to invoke itself, 
          which would be our very own clang-buils.ps1 script. 

          All this script invocation command is enveloped in single quotes. One, quick to mind solution 
          would be to use double quotes. However, this is not practical because it would lead to further 
          issues down the road since those strings are interpolated (and the $ sign is valid in a Windows file path).

          We have to keep using single quotes, but make sure that we double escape them when we find them.
          IMPORTANT: there are single quotes which we should not escape. 
          In order to precisely match the quotes that we need, we are exploiting the following detail:
          file paths containing single quotes will never have spaces to the left or right of them, but the ones we 
          are not interested in will have space either to the left or the right.
           */
          Arguments = Regex.Replace(aScript, @"([\w|\\])'([\w|\\])", "$1''$2")
        };

        //Update arguments and FileName path for Cpt alias added from pwsh
        if (SettingsProvider.CompilerSettingsModel.Powershell7)
        {
          process.StartInfo.FileName = File.Exists(GetFilePathFromEnviromentVar(ScriptConstants.kPwsh)) ?
            GetFilePathFromEnviromentVar(ScriptConstants.kPwsh) :
            $"{Environment.SystemDirectory}\\{ScriptConstants.kPowerShellPath}";
          if (aUsePwshFileName)
          {
            process.StartInfo.Arguments = "-Command \"" + process.StartInfo.Arguments + "\"";
          }
        }

        process.StartInfo.EnvironmentVariables["Path"] = CreatePathEnvironmentVariable();
        process.StartInfo.EnvironmentVariables["NUMBER_OF_PROCESSORS"] = GetNumberOfProcessors().ToString();

        var customTidyExecutable = GetCustomTidyPath();

        if (string.IsNullOrWhiteSpace(customTidyExecutable) == false)
          process.StartInfo.EnvironmentVariables[ScriptConstants.kEnvrionmentTidyPath] = customTidyExecutable;

        process.EnableRaisingEvents = true;
        process.ErrorDataReceived += DataErrorHandler;
        process.OutputDataReceived += DataHandler;
        process.Exited += ExitedHandler;
        process.Disposed += ExitedHandler;
        process.Exited += (sender, e) =>
        {
          RunController.runningProcesses.Remove(process);
        };

        RunController.runningProcesses.Add(process);

        process.Start();

        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        process.WaitForExit();
      }
      catch (Exception e)
      {
        process.EnableRaisingEvents = false;
        process.ErrorDataReceived -= DataErrorHandler;
        process.OutputDataReceived -= DataHandler;
        process.Exited -= ExitedHandler;
        process.Disposed -= ExitedHandler;

        process.Close();

        throw e;
      }
      return true;
    }

    public static void EndInteractiveMode()
    {
      if (mInteractiveProcess == null)
        return;
      RunController.runningProcesses.Kill(mInteractiveProcess);
      {
        mOutputWindowController.Write("❎ Interactive mode deactivated on document");
      }
      if (mInteractiveProcess.HasExited || mInteractiveProcess.Responding == false)
        mInteractiveProcess = null;
      mInteractiveMode = false;
    }

    public static void InvokeInteractiveMode(KeyValuePair<string, string> aKeyValuePair)
    {
      if (SettingsProvider.CompilerSettingsModel.Powershell7
        && string.IsNullOrEmpty(GetFilePathFromEnviromentVar(ScriptConstants.kPwsh)))
      {
        mOutputWindowController.Write("Can't find PowerShell 7 in PATH");
        SettingsHandler settingsHandler = new SettingsHandler();
        SettingsProvider.CompilerSettingsModel.Powershell7 = false;
        settingsHandler.SaveSettings();
        return;
      }

      mOutputWindowController.Write("✅ Interactive mode activated on document: " + aKeyValuePair.Key);

      if (mInteractiveProcess == null)
        mInteractiveProcess = new Process();
      if (mInteractiveMode)
      {
        mInteractiveProcess.StandardInput.WriteLine(InteractivCommands);
      }
      else
        try
        {
          if (RunController.StopCommandActivated)
          {
            return;
          }
          mInteractiveProcess.StartInfo = new ProcessStartInfo()
          {
            FileName = $"{Environment.SystemDirectory}\\{ScriptConstants.kPowerShellPath}",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            CreateNoWindow = true,
            UseShellExecute = false,
            Arguments = Regex.Replace(aKeyValuePair.Value, @"([\w|\\])'([\w|\\])", "$1''$2")
          };
          mInteractiveProcess.StartInfo.EnvironmentVariables["Path"] = CreatePathEnvironmentVariable();
          mInteractiveProcess.StartInfo.EnvironmentVariables["NUMBER_OF_PROCESSORS"] = GetNumberOfProcessors().ToString();

          var customTidyExecutable = GetCustomTidyPath();

          if (string.IsNullOrWhiteSpace(customTidyExecutable) == false)
            mInteractiveProcess.StartInfo.EnvironmentVariables[ScriptConstants.kEnvrionmentTidyPath] = customTidyExecutable;

          mInteractiveProcess.EnableRaisingEvents = true;
          mInteractiveProcess.ErrorDataReceived += DataErrorHandler;
          mInteractiveProcess.OutputDataReceived += DataHandler;
          mInteractiveProcess.Exited += ExitedHandler;
          mInteractiveProcess.Disposed += ExitedHandler;

          mInteractiveProcess.Start();

          mInteractiveProcess.BeginErrorReadLine();
          mInteractiveProcess.BeginOutputReadLine();

          mInteractiveProcess.StandardInput.WriteLine(MatchConstants.SetOutpuDump);
          mInteractiveProcess.StandardInput.WriteLine(InteractivCommands);
          mInteractiveMode = true;
        }
        catch (Exception e)
        {
          mInteractiveProcess.EnableRaisingEvents = false;
          mInteractiveProcess.ErrorDataReceived -= DataErrorHandler;
          mInteractiveProcess.OutputDataReceived -= DataHandler;
          mInteractiveProcess.Exited -= ExitedHandler;
          mInteractiveProcess.Disposed -= ExitedHandler;
          mInteractiveProcess.Close();
          throw e;
        }
    }


    public static void InvokePassSequentialCommands(Dictionary<string, string> aPathCommandPair)
    {
      if (SettingsProvider.CompilerSettingsModel.Powershell7
        && string.IsNullOrEmpty(GetFilePathFromEnviromentVar(ScriptConstants.kPwsh)))
      {
        mOutputWindowController.Write("Can't find PowerShell 7 in PATH");
        SettingsHandler settingsHandler = new SettingsHandler();
        SettingsProvider.CompilerSettingsModel.Powershell7 = false;
        settingsHandler.SaveSettings();
        return;
      }

      var id = CommandControllerInstance.CommandController.GetCurrentCommandId();
      int count = 0;

      mOutputWindowController.Write("Will process " + aPathCommandPair.Count + " files");
      mOutputWindowController.ResetMatchesNr();

      List<Task> tasks = new List<Task>();
      Parallel.ForEach(aPathCommandPair, pathCommand =>
      {
        Process process = new Process();
        try
        {
          if (RunController.StopCommandActivated)
          {
            return;
          }
          process.StartInfo = new ProcessStartInfo()
          {
            FileName = $"{Environment.SystemDirectory}\\{ScriptConstants.kPowerShellPath}",
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            CreateNoWindow = true,
            UseShellExecute = false,
            Arguments = Regex.Replace(pathCommand.Value, @"([\w|\\])'([\w|\\])", "$1''$2")
          };
          process.StartInfo.EnvironmentVariables["Path"] = CreatePathEnvironmentVariable();
          process.StartInfo.EnvironmentVariables["NUMBER_OF_PROCESSORS"] = GetNumberOfProcessors().ToString();

          var customTidyExecutable = GetCustomTidyPath();

          if (string.IsNullOrWhiteSpace(customTidyExecutable) == false)
            process.StartInfo.EnvironmentVariables[ScriptConstants.kEnvrionmentTidyPath] = customTidyExecutable;

          process.EnableRaisingEvents = true;
          process.ErrorDataReceived += DataErrorHandler;
          process.OutputDataReceived += DataHandler;
          process.Exited += ExitedHandler;
          process.Disposed += ExitedHandler;
          Interlocked.Increment(ref count);
          mOutputWindowController.Write($"{count}: {pathCommand.Key}");

          //Display pathCommand value, if is in verbose mode
          if (SettingsProvider.CompilerSettingsModel.VerboseMode)
          {
            mOutputWindowController.Write($"{count}: {pathCommand.Value}");
          }

          RunController.runningProcesses.Add(process);

          process.Start();

          process.BeginErrorReadLine();
          process.BeginOutputReadLine();

          process.WaitForExit();
        }
        catch (Exception e)
        {
          process.EnableRaisingEvents = false;
          process.ErrorDataReceived -= DataErrorHandler;
          process.OutputDataReceived -= DataHandler;
          process.Exited -= ExitedHandler;
          process.Disposed -= ExitedHandler;
          process.Close();
          throw e;
        }
      });
      mOutputWindowController.WriteMatchesNr();
    }

    /// <summary>
    /// Get the number of processors that a process will run on
    /// depend on environment var, NUMBER_OF_PROCESSORS and
    /// percentage of CPU Limit choosed by user
    /// </summary>
    /// <returns></returns>
    private static int GetNumberOfProcessors()
    {
      int processorsNumber = int.Parse(Environment.GetEnvironmentVariable("NUMBER_OF_PROCESSORS"));
      int cpuLimit = SettingsProvider.CompilerSettingsModel.CpuLimit;
      return (cpuLimit * processorsNumber) / 100;
    }

    public static string CreatePathEnvironmentVariable()
    {
      var path = Environment.GetEnvironmentVariable("Path");
      var llvmModel = SettingsProvider.LlvmSettingsModel;

      if (string.IsNullOrEmpty(llvmModel.LlvmSelectedVersion)) return path;

      var llvmVersions = llvmModel.LlvmSelectedVersion.Split('.');
      // for parallel execution llvm need to be >= 13.0.1
      if ((Int16.Parse(llvmVersions[0]) >= 13))
      {
        if((Int16.Parse(llvmVersions[0]) == 13) && (Int16.Parse(llvmVersions[1]) == 0) && (Int16.Parse(llvmVersions[2]) == 0))
        {
          return path;
        }
      }
      else
      {
        return path;
      }

      var paths = path.Split(';').ToList();
      paths.RemoveAt(paths.Count - 1);
      paths.RemoveAll(ContainsLlvm);

      if (string.IsNullOrWhiteSpace(llvmModel.PreinstalledLlvmPath) == false
        && llvmModel.LlvmSelectedVersion == llvmModel.PreinstalledLlvmVersion)
      {
        paths.Add(llvmModel.PreinstalledLlvmPath);
      }
      else
      {
        paths.Add(GetUsedLlvmVersionPath(llvmModel.LlvmSelectedVersion));
      }

      return String.Join(";", paths);
    }

    /// <summary>
    /// Run a process that download a tool (and returns path) if it wasn't found on disk
    /// </summary>
    /// <exception cref="Exception"></exception>
    public static string DownloadTool(string tool)
    {
      var getllvmScriptPath = GetLLVMScriptFilePath();

      Process process = new Process();
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.RedirectStandardInput = true;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.EnvironmentVariables["Path"] = PowerShellWrapper.CreatePathEnvironmentVariable();
      process.StartInfo.EnvironmentVariables["NUMBER_OF_PROCESSORS"] = GetNumberOfProcessors().ToString();
      process.StartInfo.FileName = $"{Environment.SystemDirectory}\\{ScriptConstants.kPowerShellPath}";

      //Check if powershell 7 is in Path
      string powershell = string.Empty;
      if (SettingsProvider.CompilerSettingsModel.Powershell7)
      {
        powershell = ScriptConstants.kScriptBeginning;
        if (string.IsNullOrEmpty(GetFilePathFromEnviromentVar(ScriptConstants.kPwsh)))
        {
          mOutputWindowController.Write("Can't find PowerShell 7 in PATH");
          SettingsHandler settingsHandler = new SettingsHandler();
          SettingsProvider.CompilerSettingsModel.Powershell7 = false;
          settingsHandler.SaveSettings();
          return string.Empty;
        }
      }
      else
      {
        powershell = ScriptConstants.kScriptBeginning;
      }
      process.StartInfo.Arguments = powershell + $" '{getllvmScriptPath}' {tool} \"";

      RunController.runningProcesses.Add(process);

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


    /// <summary>
    /// Return file path, if is found in PATH enviroment variables. 
    /// Returns empty string if path can't be found. 
    /// </summary>
    /// <returns></returns>
    public static string GetFilePathFromEnviromentVar(string aFile)
    {
      var path = Environment.GetEnvironmentVariable("Path");
      var paths = path.Split(';').ToList();

      //Is file in path - first check
      string filePath = paths.Find(p => p.Contains(ScriptConstants.kPowershell7PathPart))?.ToString();
      if (!string.IsNullOrWhiteSpace(filePath))
      {
        //Check if file exists on this path
        string absoluteFilePath = Path.Combine(filePath, aFile);
        if (File.Exists(absoluteFilePath))
          return absoluteFilePath;
      }

      //Search file in all paths - second check
      var pathResult = paths.Find(p => File.Exists(Path.Combine(p, aFile)));
      if (!string.IsNullOrEmpty(pathResult))
      {
        string absoluteFilePath = Path.Combine(paths.Find(p => File.Exists(Path.Combine(p, aFile))).ToString(), aFile);
        if (File.Exists(absoluteFilePath))
          return absoluteFilePath;
      }
      return string.Empty;
    }

    #endregion


    #region Private Methods

    private static string GetLLVMScriptFilePath()
    {
      var assemblyPath = Assembly.GetExecutingAssembly().Location;
      var scriptDirectory = assemblyPath.Substring(0, assemblyPath.LastIndexOf('\\'));

      return Path.Combine(scriptDirectory, "Tooling\\v1\\psClang", ScriptConstants.kGetLLVMScriptName);
    }

    private static string GetCustomTidyPath()
    {
      var executablePath = SettingsProvider.TidySettingsModel.CustomExecutable;

      return string.IsNullOrWhiteSpace(executablePath) == false ? executablePath : string.Empty;
    }

    public static string GetClangBuildScriptPath()
    {
      var assemblyPath = Assembly.GetExecutingAssembly().Location;
      var scriptDirectory = assemblyPath.Substring(0, assemblyPath.LastIndexOf('\\'));

      return Path.Combine(scriptDirectory, ScriptConstants.ToolingV1, ScriptConstants.kScriptName);
    }

    private static string GetUsedLlvmVersionPath(string llvmVersion)
    {
      var settingsPathBuilder = new SettingsPathBuilder();
      return settingsPathBuilder.GetLlvmBinPath(llvmVersion);
    }

    private static bool ContainsLlvm(string input)
    {
      return input.ToLower().Contains("llvm");
    }
    #endregion

  }
}

