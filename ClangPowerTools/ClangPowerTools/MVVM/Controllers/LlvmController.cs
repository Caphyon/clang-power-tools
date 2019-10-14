using ClangPowerTools.Helpers;
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
    public EventHandler InstallFinished { get; set; }
    public EventHandler UninstallFinished { get; set; }

    private const string installExeParameters = "/S /D=";
    private const string uninstallExeParameters = "/S";
    private const string arguments = @"/C reg delete HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\LLVM /f &";
    private const string processFileName = "cmd.exe";
    private const string processVerb = "runas";
    private const string llvmReleasesUri = @"http://releases.llvm.org";
    private const string llvm = "LLVM";
    private const string uninstall = "Uninstall";

    private Process process;
 
    private readonly SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
    private readonly FileSystem fileSystem = new FileSystem();
    private string bitOperatingSystem = string.Empty;
    #endregion


    #region Public Methods
    public void Download(string version, DownloadProgressChangedEventHandler method)
    {
      CreateDirectory(version);

      var executablePath = settingsPathBuilder.GetLlvmExecutablePath(version, llvm + version);
      var uri = string.Concat(llvmReleasesUri, "/", version, "/", llvm, "-", version, GetOperatingSystemParamaters());

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
        process.Exited += new EventHandler(InstallProcessExited);
        process.Exited += new EventHandler(InstallFinished);
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
      if (IsVersionExeOnDisk(version, uninstall) == false)
      {
        setUninstallCommandState();
        DeleteDirectory(version);
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

    public bool IsVersionExeOnDisk(string version, string name)
    {
      var executablePath = settingsPathBuilder.GetLlvmExecutablePath(version, name);
      return File.Exists(executablePath);
    }

    public void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
    {
      llvmModel.DownloadProgress = 0;
      downloadCancellationToken.Dispose();
      downloadCancellationToken = new CancellationTokenSource();

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
      setUninstallCommandState();
      DeleteDirectory(llvmModel.Version);
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
      var exeName = string.Concat(llvm, llvmModel.Version, ".exe");
      var path = Path.Combine(settingsPathBuilder.GetLlvmPath(version), exeName);
      fileSystem.DeleteFile(path);
    }

    private string GetOperatingSystemParamaters()
    {
      if (Environment.Is64BitOperatingSystem)
      {
        bitOperatingSystem = "-win64.exe";
      }
      else
      {
        bitOperatingSystem = "-win32.exe";
      }

      return bitOperatingSystem;
    }

    #endregion
  }
}
