using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace ClangPowerTools.MVVM.Controllers
{
  public class LlvmController
  {
    public LlvmSettingsModel llvmModel = new LlvmSettingsModel();
    public delegate void SetInstallCommandState();
    public delegate void SetUninstallCommandState();
    public SetInstallCommandState setInstallCommandState;
    public SetUninstallCommandState setUninstallCommandState;
    public EventHandler InstallFinished;
    public EventHandler UninstallFinished;

    private const string installExeParameters = "/S /D=";
    private const string uninstallExeParameters = "/S";
    private const string arguments = @"/C reg delete HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\LLVM /f &";
    private const string processFileName = "cmd.exe";
    private const string processVerb = "runas";
    private const string uri = @"http://releases.llvm.org";
    private const string llvm = "LLVM";
    private const string uninstall = "Uninstall";

    private Process process;
    private CancellationTokenSource downloadCancellationToken = new CancellationTokenSource();
    private SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();


    public void InstallLlVmVersion(string version)
    {
      var llVmVersionPath = settingsPathBuilder.GetLlvmPath(version);
      var executablePath = settingsPathBuilder.GetLlvmExecutablePath(version, llvm + version);
      var startInfoArguments = string.Concat(arguments, " ", executablePath, " ", installExeParameters, llVmVersionPath);

      try
      {
        process = new Process();
        process.StartInfo.FileName = processFileName;
        process.StartInfo.Arguments = startInfoArguments;
        process.StartInfo.Verb = processVerb;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.EnableRaisingEvents = true;
        //TODO clear events or try using
        process.Exited += new EventHandler(InstallProcessExited);
        process.Exited += new EventHandler(InstallFinished);
        process.Start();

      }
      catch (Exception e)
      {
        setInstallCommandState();
        DeleteLlvmVersion(llvmModel.Version);
        MessageBox.Show(e.Message, "Installation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void InstallProcessExited(object sender, EventArgs e)
    {
      process.Close();
      var exeName = string.Concat("LLVM", llvmModel.Version, ".exe");
      DeleteExe(llvmModel.Version, exeName);
      setInstallCommandState();
    }

    public void DownloadLlvmVersion(string version, DownloadProgressChangedEventHandler method)
    {
      CreateVersionFolder(version);

      //TODO check windows version
      var executablePath = settingsPathBuilder.GetLlvmExecutablePath(version, llvm + version);
      var finalUri = string.Concat(uri, "/", version, "/", llvm, "-", version, "-win64.exe");

      using (var client = new WebClient())
      {
        client.DownloadProgressChanged += method;
        client.DownloadFileCompleted += DownloadFileCompleted;
        downloadCancellationToken.Token.Register(client.CancelAsync);
        client.DownloadFileAsync(new Uri(finalUri), executablePath);
      }
    }

    private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
      if (downloadCancellationToken.IsCancellationRequested || llvmModel.DownloadProgress != llvmModel.MaxProgress)
      {
        setUninstallCommandState();
        DeleteLlvmVersion(llvmModel.Version);
        MessageBox.Show("The download process has stopped.", "Download", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
      else
      {
        llvmModel.IsInstalling = true;
        llvmModel.IsDownloading = false;
        InstallLlVmVersion(llvmModel.Version);
      }

      llvmModel.DownloadProgress = 0;
      downloadCancellationToken.Dispose();
      downloadCancellationToken = new CancellationTokenSource();
    }

    public void UninstallLlvmVersion(string version)
    {
      if (IsVersionExeOnDisk(version, uninstall) == false)
      {
        setUninstallCommandState();
        DeleteLlvmVersion(version);
        return;
      }

      try
      {
        process = new Process();
        process.StartInfo.FileName = settingsPathBuilder.GetLlvmExecutablePath(version, uninstall);
        process.StartInfo.Arguments = uninstallExeParameters;
        process.StartInfo.Verb = processVerb;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.EnableRaisingEvents = true;
        process.Exited += new EventHandler(UninstallProcessExited);
        process.Exited += new EventHandler(UninstallFinished);
        process.Start();
      }
      catch (Exception e)
      {
        setUninstallCommandState();
        MessageBox.Show(e.Message, "Uninstall Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void UninstallProcessExited(object sender, EventArgs e)
    {
      process.Close();
      setUninstallCommandState();
      DeleteLlvmVersion(llvmModel.Version);
    }

    public bool IsVersionExeOnDisk(string version, string name)
    {
      var executablePath = settingsPathBuilder.GetLlvmExecutablePath(version, name);
      return File.Exists(executablePath);
    }


    private void DeleteLlvmVersion(string version)
    {
      var path = settingsPathBuilder.GetLlvmPath(version);
      Directory.Delete(path, true);
    }

    private void CreateVersionFolder(string version)
    {
      var path = settingsPathBuilder.GetLlvmPath(version);
      Directory.CreateDirectory(path);
    }

    private void DeleteExe(string version, string exeName)
    {
      var path = Path.Combine(settingsPathBuilder.GetLlvmPath(version), exeName);
      File.Delete(path);
    }
  }
}
