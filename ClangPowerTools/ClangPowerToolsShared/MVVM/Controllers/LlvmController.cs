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

    public LlvmModel llvmModel = new LlvmModel();
    public CancellationTokenSource downloadCancellationToken = new CancellationTokenSource();

    public delegate void OnOperationCanceled();
    public event OnOperationCanceled OnOperationCanceldEvent;

    private Process process;
    private readonly SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
    
    private const string registryLlvmUninstall = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\LLVM";
    private const string keyDisplayVersion = "DisplayVersion";
    private const string registryLlvm = @"SOFTWARE\WOW6432Node\LLVM\LLVM";
    private const string keyInstallPath = "";

    #endregion


    #region Properties

    public EventHandler InstallFinished { get; set; }
    public EventHandler UninstallFinished { get; set; }
    public CancelEventHandler SettingsWindowClosed { get; set; }

    #endregion

    #region Constructor

    public LlvmController()
    {
      SettingsWindowClosed += WindowClosed;
    }

    #endregion

    #region Public Methods

    public string GetLlvmPathFromRegistry()
    {
      var registryUtility = new RegistryUtility(registryLlvm);
      var path = registryUtility.ReadLocalMachineKey(keyInstallPath);

      if (path == null) return string.Empty;

      return Path.Combine(path, "bin");
    }

    public string GetVersionFromRegistry()
    {
      var registryUtility = new RegistryUtility(registryLlvmUninstall);
      var version = registryUtility.ReadLocalMachineKey(keyDisplayVersion);

      if (version == null) return string.Empty;

      return version;
    }

    public void Download(string version, DownloadProgressChangedEventHandler method)
    {
      CreateVersionDirectory(version);

      var llvmUri = new LlvmUri();
      var executablePath = settingsPathBuilder.GetLlvmExecutablePath(version, LlvmConstants.Llvm + version);
      var uri = string.Empty;

      if (LlvmVersionsAlternate.VersionAlternateUri.Contains(version))
      {
        uri = llvmUri.GetGitHubUri(version);
      }
      else
      {
        uri = llvmUri.GetDefaultUri(version);
      }

      try
      {
        using var client = new WebClient();
        client.DownloadProgressChanged += method;
        client.DownloadFileCompleted += DownloadCompleted;
        downloadCancellationToken.Token.Register(client.CancelAsync);
        client.DownloadFileAsync(new Uri(uri), executablePath);
      }
      catch (Exception)
      {
        DownloadCanceled();
      }
    }

    public void Install(string version)
    {
      var llVmVersionPath = settingsPathBuilder.GetLlvmPath(version);
      var executablePath = settingsPathBuilder.GetLlvmExecutablePath(version, LlvmConstants.Llvm + version);
      var startInfoArguments = string.Concat(LlvmConstants.Arguments, " ", "\"", executablePath, "\"", " ", LlvmConstants.InstallExeParameters, llVmVersionPath);

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
        DefaultState();
        if (OnOperationCanceldEvent != null)
          OnOperationCanceldEvent();
        DeleteLlvmDirectory(llvmModel.Version);
        MessageBox.Show(e.Message, "Installation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    public void Uninstall(string version)
    {
      if (DoesUninstallExist(version) == false)
      {
        DeleteLlvmDirectory(version);
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
        InstallFinishedState();
        if (OnOperationCanceldEvent != null)
          OnOperationCanceldEvent();
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
        DownloadCanceled();
      }
      else
      {
        BeginInstallation();
      }
    }

    #endregion

    #region Private Methods

    private void InstallFinishedState()
    {
      llvmModel.IsInstalled = true;
      llvmModel.IsInstalling = false;
    }

    private void InstallingState()
    {
      llvmModel.IsInstalling = true;
      llvmModel.IsDownloading = false;
    }

    private void DefaultState()
    {
      llvmModel.IsInstalled = false;
      llvmModel.IsDownloading = false;
      llvmModel.IsInstalling = false;
    }

    private void DownloadCanceled()
    {
      DefaultState();
      if (OnOperationCanceldEvent != null)
        OnOperationCanceldEvent();
      DeleteLlvmDirectory(llvmModel.Version);
      ResetDownloadProgressState();
      MessageBox.Show("The download process has stopped.", "LLVM Download", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void BeginInstallation()
    {
      InstallingState();
      ResetDownloadProgressState();
      Install(llvmModel.Version);
    }

    private void ResetDownloadProgressState()
    {
      llvmModel.DownloadProgress = 0;
      downloadCancellationToken.Dispose();
      downloadCancellationToken = new CancellationTokenSource();
    }

    private void InstallProcessExited(object sender, EventArgs e)
    {
      process.Close();
      DeleteInstallerFile(llvmModel.Version);
      InstallFinishedState();
    }

    private void UninstallProcessExited(object sender, EventArgs e)
    {
      process.Close();
      DeleteLlvmDirectory(llvmModel.Version);
      DefaultState();
    }

    private void WindowClosed(object sender, EventArgs e)
    {
      if (llvmModel.DownloadProgress > 0 && llvmModel.DownloadProgress != llvmModel.MaxProgress)
      {
        downloadCancellationToken.Cancel();
      }
      SettingsWindowClosed -= WindowClosed;
    }

    private void CreateVersionDirectory(string version)
    {
      var path = settingsPathBuilder.GetLlvmPath(version);
      FileSystem.CreateDirectory(path);
    }

    private void DeleteLlvmDirectory(string version)
    {
      var path = settingsPathBuilder.GetLlvmPath(version);
      FileSystem.DeleteDirectory(path);
    }

    private void DeleteInstallerFile(string version)
    {
      var exeName = string.Concat(LlvmConstants.Llvm, llvmModel.Version, ".exe");
      var path = Path.Combine(settingsPathBuilder.GetLlvmPath(version), exeName);
      FileSystem.DeleteFile(path);
    }

    private bool DoesUninstallExist(string version)
    {
      return IsVersionExeOnDisk(version, LlvmConstants.Uninstall);
    }

    #endregion
  }
}
