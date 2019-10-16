using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Constants;
using ClangPowerTools.MVVM.Interfaces;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace ClangPowerTools.MVVM.Controllers
{
  public class LlvmController : IDownload, IInstall
  {
    #region Members

    public LlvmSettingsModel llvmModel = new LlvmSettingsModel();
    public CancellationTokenSource downloadCancellationToken = new CancellationTokenSource();
    
    public delegate void SetInstallCommandState();
    public delegate void SetUninstallCommandState();
    public SetInstallCommandState setInstallCommandState;
    public SetUninstallCommandState setUninstallCommandState;

    private Process process;
    private readonly SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
    private readonly FileSystem fileSystem = new FileSystem();

    #endregion


    #region Properties

    public EventHandler InstallFinished { get; set; }
    public EventHandler UninstallFinished { get; set; }

    #endregion


    #region Public Methods

    public void Download(string version, DownloadProgressChangedEventHandler method)
    {
      CreateDirectory(version);

      var executablePath = settingsPathBuilder.GetLlvmExecutablePath(version, LlvmConstants.Llvm + version);
      var uri = string.Concat(LlvmConstants.LlvmReleasesUri, "/", version, "/", LlvmConstants.Llvm, "-", version, GetOperatingSystemParamaters());

      using (var client = new WebClient())
      {
        client.DownloadProgressChanged += method;
        client.DownloadFileCompleted += DownloadCompleted;
        downloadCancellationToken.Token.Register(client.CancelAsync);
        client.DownloadFileAsync(new Uri(uri), executablePath);
      }
    }

    public void Install(string version)
    {
      var llVmVersionPath = settingsPathBuilder.GetLlvmPath(version);
      var executablePath = settingsPathBuilder.GetLlvmExecutablePath(version, LlvmConstants.Llvm + version);
      var startInfoArguments = string.Concat(LlvmConstants.Arguments, " ", executablePath, " ", LlvmConstants.InstallExeParameters, llVmVersionPath);

      try
      {
        process = new Process();
        process.StartInfo.FileName = LlvmConstants.ProcessFileName;
        process.StartInfo.Arguments = startInfoArguments;
        process.StartInfo.Verb = LlvmConstants.ProcessVerb;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.EnableRaisingEvents = true;
        process.Exited += InstallProcessExited;
        process.Exited += InstallFinished;
        process.Start();

      }
      catch (Exception e)
      {
        setInstallCommandState();
        process.Kill();
        DeleteDirectory(llvmModel.Version);
        MessageBox.Show(e.Message, "Installation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    public void Uninstall(string version)
    {
      if (IsVersionExeOnDisk(version, LlvmConstants.Uninstall) == false)
      {
        setUninstallCommandState();
        DeleteDirectory(version);
        return;
      }

      try
      {
        process = new Process();
        process.StartInfo.FileName = settingsPathBuilder.GetLlvmExecutablePath(version, LlvmConstants.Uninstall);
        process.StartInfo.Arguments = LlvmConstants.UninstallExeParameters;
        process.StartInfo.Verb = LlvmConstants.ProcessVerb;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.EnableRaisingEvents = true;
        process.Exited += UninstallProcessExited;
        process.Exited += UninstallFinished;
        process.Start();
      }
      catch (Exception e)
      {
        setUninstallCommandState();
        MessageBox.Show(e.Message, "Uninstall Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    public bool IsVersionExeOnDisk(string version, string name)
    {
      var executablePath = settingsPathBuilder.GetLlvmExecutablePath(version, name);
      return File.Exists(executablePath);
    }

    public void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
    {
      if (downloadCancellationToken.IsCancellationRequested || llvmModel.DownloadProgress != llvmModel.MaxProgress)
      {
        setUninstallCommandState();
        DeleteDirectory(llvmModel.Version);
        MessageBox.Show("The download process has stopped.", "Download", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
      else
      {
        llvmModel.IsInstalling = true;
        llvmModel.IsDownloading = false;
        Install(llvmModel.Version);
      }

      llvmModel.DownloadProgress = 0;
      downloadCancellationToken.Dispose();
      downloadCancellationToken = new CancellationTokenSource();
    }

    #endregion


    #region Private Methods

    private void InstallProcessExited(object sender, EventArgs e)
    {
      process.Close();
      DeleteFile(llvmModel.Version);
      setInstallCommandState();
    }

    private void UninstallProcessExited(object sender, EventArgs e)
    {
      process.Close();
      DeleteDirectory(llvmModel.Version);
      setUninstallCommandState();
    }

    private void CreateDirectory(string version)
    {
      var path = settingsPathBuilder.GetLlvmPath(version);
      fileSystem.CreateDirectory(path);
    }

    private void DeleteDirectory(string version)
    {
      var path = settingsPathBuilder.GetLlvmPath(version);
      fileSystem.DeleteDirectory(path);
    }

    private void DeleteFile(string version)
    {
      var exeName = string.Concat(LlvmConstants.Llvm, llvmModel.Version, ".exe");
      var path = Path.Combine(settingsPathBuilder.GetLlvmPath(version), exeName);
      fileSystem.DeleteFile(path);
    }

    private string GetOperatingSystemParamaters()
    {
      return Environment.Is64BitOperatingSystem ? LlvmConstants.Os64Paramater : LlvmConstants.Os32Paramater;
    }

    #endregion
  }
}
