using ClangPowerTools.MVVM.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class LlvmSettingsViewModel : INotifyPropertyChanged
  {
    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    private const string installExeParameters = "/S /D=";
    private const string uninstallExeParameters = "/S";
    private const string arguments = @"/C reg delete HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\LLVM /f &";
    private const string processFileName = "cmd.exe";
    private const string processVerb = "runas";
    private const string uri = @"http://releases.llvm.org";
    private const string llvm = "LLVM";
    private const string uninstall = "Uninstall";

    private Process process;
    private List<LlvmModel> llvms;
    private LlvmModel selectedLlvm = new LlvmModel();
    private CancellationTokenSource cancellationToken = new CancellationTokenSource();
    private bool canDownload = true;
    private string appdDataPath;

    #endregion

    #region Constructor
    public LlvmSettingsViewModel()
    {
      var settingsPathBuilder = new SettingsPathBuilder();
      appdDataPath = settingsPathBuilder.GetPath("");
      IntitializeView();
    }
    #endregion

    #region Properties

    public List<LlvmModel> Llvms
    {
      get
      {
        return llvms;
      }

      set
      {
        llvms = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Llvms"));
      }

    }

    #endregion


    #region Commands
    public void DownloadCommand(int elementIndex)
    {
      if (canDownload)
      {
        canDownload = false;
        selectedLlvm = llvms[elementIndex];
        selectedLlvm.IsDownloading = true;
        DownloadLlvmVersion(selectedLlvm.Version);
      }
    }

    public void CancelCommand(int elementIndex)
    {
      if (selectedLlvm.IsDownloading)
      {
        canDownload = true;
        selectedLlvm.DownloadProgress = 0;
        selectedLlvm.IsDownloading = false;
        cancellationToken.Cancel();
        ForceCloseProcessIfRequired(selectedLlvm.Version);
      }
    }

    public void UninstallCommand(int elementIndex)
    {
      selectedLlvm = llvms[elementIndex];
      if (canDownload == true && selectedLlvm.IsInstalled)
      {
        canDownload = false;
        UninstallLlvmVersion(selectedLlvm.Version);
      }
    }

    #endregion



    #region Private Methods

    private void DownloadLlvmVersion(string version)
    {
      CreateVersionFolder(version);

      var executablePath = GetLlvmExecutablePath(version, llvm + version);
      var finalUri = string.Concat(uri, "/", version, "/", llvm, "-", version, "-win64.exe");

      using (var client = new WebClient())
      {
        client.DownloadProgressChanged += DownloadProgressChanged;
        client.DownloadFileCompleted += DownloadFileCompleted;
        cancellationToken.Token.Register(client.CancelAsync);
        client.DownloadFileAsync(new Uri(finalUri), executablePath);
      }
    }

    private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
      if (cancellationToken.IsCancellationRequested)
      {
        DeleteLlvmVersion(selectedLlvm.Version);
      }
      else
      {
        InstallLlVmVersion(selectedLlvm.Version);
      }

      cancellationToken.Dispose();
      cancellationToken = new CancellationTokenSource();
    }


    private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      selectedLlvm.DownloadProgress = e.ProgressPercentage;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedLlvm"));
    }


    private void InstallLlVmVersion(string version)
    {
      var llVmVersionPath = GetLlVmVersionPath(version);
      var executablePath = GetLlvmExecutablePath(version, llvm + version);
      var startInfoArguments = string.Concat(arguments, " ", executablePath, " ", installExeParameters, llVmVersionPath);

      try
      {
        using (var process = new Process())
        {
          process.StartInfo.FileName = processFileName;
          process.StartInfo.Arguments = startInfoArguments;
          process.StartInfo.Verb = processVerb;
          process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
          process.EnableRaisingEvents = true;
          process.Exited += new EventHandler(InstallProcessExited);
          process.Start();
        }
      }
      catch (Exception e)
      {

        MessageBox.Show(e.Message, "Installation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void InstallProcessExited(object sender, EventArgs e)
    {
      selectedLlvm.IsInstalled = true;
      canDownload = true;
    }

    private void UninstallLlvmVersion(string version)
    {
      var executablePath = GetLlvmExecutablePath(version, uninstall);
      if (Directory.Exists(executablePath) == false)
      {
        DeleteLlvmVersion(version);
        return;
      }

      try
      {
        using (var process = new Process())
        {
          process.StartInfo.FileName = executablePath;
          process.StartInfo.Arguments = uninstallExeParameters;
          process.StartInfo.Verb = processVerb;
          process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
          process.EnableRaisingEvents = true;
          process.Exited += new EventHandler(UninstallProcessExited);
          process.Start();
        }
      }
      catch (Exception e)
      {

        MessageBox.Show(e.Message, "Uninstall Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void UninstallProcessExited(object sender, EventArgs e)
    {
      DeleteLlvmVersion(selectedLlvm.Version);
    }

    private void DeleteLlvmVersion(string version)
    {
      canDownload = true;
      selectedLlvm.IsInstalled = false;
      selectedLlvm.IsDownloading = false;

      var path = GetLlVmVersionPath(version);
      Directory.Delete(path, true);
    }

    private string GetLlVmVersionPath(string version)
    {
      var folderName = string.Concat(llvm, version);
      return Path.Combine(appdDataPath, llvm, folderName);
    }

    private string GetLlvmExecutablePath(string version, string executableName)
    {
      var path = GetLlVmVersionPath(version);
      return string.Concat(path, "\\", executableName, ".exe");
    }

    private void CreateVersionFolder(string version)
    {
      var path = GetLlVmVersionPath(version);
      Directory.CreateDirectory(path);
    }

    private void IntitializeView()
    {
      llvms = new List<LlvmModel>();

      foreach (var version in LlvmVersions.Versions)
      {
        var llvmModel = new LlvmModel()
        {
          Version = version,
          IsInstalled = CheckVersionOnDisk(version),
          IsSelected = false,
        };

        llvms.Add(llvmModel);
      }
    }

    private void ForceCloseProcessIfRequired(string version)
    {
      var executablePath = GetLlvmExecutablePath(version, llvm+version);
      if (File.Exists(executablePath))
      {
        process.Kill();
        process.Dispose();
      }
    }

    private bool CheckVersionOnDisk(string version)
    {
      var path = GetLlVmVersionPath(version);
      if (Directory.Exists(path)) return true;

      return false;
    }
    #endregion
  }
}
