﻿using System;
using System.Diagnostics;
using System.Linq;

namespace ClangPowerTools.Commands.BackgroundTidy
{
  public class PowerShellWrapperBackground
  {
    #region Properties

    public DataReceivedEventHandler DataErrorHandler { get; set; }
    public DataReceivedEventHandler DataHandler { get; set; }
    public EventHandler ExitedHandler { get; set; }

    #endregion

    #region Public Methods

    public void Invoke(string script, RunningProcesses runningProcesses)
    {
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
          Arguments = script,
        };
        process.StartInfo.EnvironmentVariables["Path"] = CreatePathEnvironmentVariable();

        process.EnableRaisingEvents = true;
        process.ErrorDataReceived += DataErrorHandler;
        process.OutputDataReceived += DataHandler;
        process.Exited += ExitedHandler;
        process.Disposed += ExitedHandler;

        runningProcesses.Add(process);

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
    }

    #endregion


    #region Private Methods
    private string CreatePathEnvironmentVariable()
    {
      var path = Environment.GetEnvironmentVariable("Path");
      var llvmModel = SettingsProvider.LlvmSettingsModel;

      if (string.IsNullOrEmpty(llvmModel.LlvmSelectedVersion)) return path;

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
