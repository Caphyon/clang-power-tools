using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

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
    private CompilerSettingsModel compilerModel = new CompilerSettingsModel();
    private SettingsProvider settingsProvider = new SettingsProvider();
    private List<LlvmSettingsModel> llvms = new List<LlvmSettingsModel>();
    private LlvmSettingsModel selectedLlvm = new LlvmSettingsModel();
    private CancellationTokenSource downloadCancellationToken = new CancellationTokenSource();
    private SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
    private bool canUseCommand = true;

    #endregion

    #region Constructor
    public LlvmSettingsViewModel()
    {
      IntitializeView();
    }
    #endregion

    #region Properties

    public List<LlvmSettingsModel> Llvms
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

    public ObservableCollection<string> InstalledLlvms { get; set; } = new ObservableCollection<string>();

    public string VersionUsed
    {
      get
      {
        return compilerModel.LlvmVersion;
      }

      set
      {
        compilerModel.LlvmVersion = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VersionUsed"));
      }
    }

    #endregion


    #region Commands
    public void DownloadCommand(int elementIndex)
    {
      if (canUseCommand)
      {
        canUseCommand = false;
        selectedLlvm = llvms[elementIndex];
        selectedLlvm.IsDownloading = true;
        DownloadLlvmVersion(selectedLlvm.Version);
      }
    }

    public void CancelCommand(int elementIndex)
    {
      if (selectedLlvm.IsDownloading)
      {
        canUseCommand = true;
        selectedLlvm.DownloadProgress = 0;
        selectedLlvm.IsDownloading = false;
        downloadCancellationToken.Cancel();
      }
    }

    public void UninstallCommand(int elementIndex)
    {
      if (canUseCommand)
      {
        canUseCommand = false;
        selectedLlvm = llvms[elementIndex];
        UninstallLlvmVersion(selectedLlvm.Version);
      }
    }

    #endregion


    #region Private Methods

    private void IntitializeView()
    {
      foreach (var version in LlvmVersions.Versions)
      {
        var llvmModel = new LlvmSettingsModel()
        {
          Version = version,
          IsInstalled = IsVersionExeOnDisk(version, uninstall),
        };

        if (llvmModel.IsInstalled)
        {
          InstalledLlvms.Add(llvmModel.Version);
        }

        llvms.Add(llvmModel);
      }

      compilerModel = settingsProvider.GetCompilerSettingsModel();
      ResetCompilerModelLlvmVersion();
    }

    private void ResetCompilerModelLlvmVersion()
    {
      if (InstalledLlvms.Count == 0)
      {
        VersionUsed = string.Empty;
      }
    }

    private void DownloadLlvmVersion(string version)
    {
      CreateVersionFolder(version);

      var executablePath = settingsPathBuilder.GetLlvmExecutablePath(version, llvm + version);
      var finalUri = string.Concat(uri, "/", version, "/", llvm, "-", version, "-win64.exe");

      using (var client = new WebClient())
      {
        client.DownloadProgressChanged += DownloadProgressChanged;
        client.DownloadFileCompleted += DownloadFileCompleted;
        downloadCancellationToken.Token.Register(client.CancelAsync);
        client.DownloadFileAsync(new Uri(finalUri), executablePath);
      }
    }


    private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
      if (downloadCancellationToken.IsCancellationRequested || selectedLlvm.DownloadProgress != selectedLlvm.MaxProgress)
      {
        SetUninstallCommandState();
        DeleteLlvmVersion(selectedLlvm.Version);
        MessageBox.Show("The download process has stopped.", "Download", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
      else
      {
        selectedLlvm.IsInstalling = true;
        selectedLlvm.IsDownloading = false;
        InstallLlVmVersion(selectedLlvm.Version);
      }

      selectedLlvm.DownloadProgress = 0;
      downloadCancellationToken.Dispose();
      downloadCancellationToken = new CancellationTokenSource();
    }


    private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      selectedLlvm.DownloadProgress = e.ProgressPercentage;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedLlvm"));
    }


    private void InstallLlVmVersion(string version)
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
        process.Start();

      }
      catch (Exception e)
      {
        SetInstallCommandState();
        DeleteLlvmVersion(selectedLlvm.Version);
        MessageBox.Show(e.Message, "Installation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void InstallProcessExited(object sender, EventArgs e)
    {
      process.Close();
      var exeName = string.Concat("LLVM", selectedLlvm.Version, ".exe");
      DeleteExe(selectedLlvm.Version, exeName);
      SetInstallCommandState();
#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
      System.Windows.Application.Current.Dispatcher.Invoke(
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs
        new Action(() =>
        {
          InsertVersionToInstalledLlvms();
        }));
    }

    private void InsertVersionToInstalledLlvms()
    {
      if (InstalledLlvms.Count == 0)
      {
        InstalledLlvms.Add(selectedLlvm.Version);
        return;
      }

      for (int i = 0; i < InstalledLlvms.Count; i++)
      {
        if (string.CompareOrdinal(selectedLlvm.Version, InstalledLlvms[i]) > 0)
        {
          InstalledLlvms.Insert(i, selectedLlvm.Version);
          break;
        }
      }
    }

    private void SetInstallCommandState()
    {
      selectedLlvm.IsInstalled = true;
      selectedLlvm.IsInstalling = false;
      canUseCommand = true;
    }

    private void UninstallLlvmVersion(string version)
    {
      if (IsVersionExeOnDisk(version, uninstall) == false)
      {
        SetUninstallCommandState();
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
        process.Start();
      }
      catch (Exception e)
      {
        SetUninstallCommandState();
        MessageBox.Show(e.Message, "Uninstall Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void UninstallProcessExited(object sender, EventArgs e)
    {
      process.Close();
      SetUninstallCommandState();
      ResetCompilerModelLlvmVersion();

#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
      System.Windows.Application.Current.Dispatcher.Invoke(
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs
        new Action(() =>
        {
          InstalledLlvms.Remove(selectedLlvm.Version);
        }));
      DeleteLlvmVersion(selectedLlvm.Version);
    }

    private void DeleteLlvmVersion(string version)
    {
      var path = settingsPathBuilder.GetLlvmPath(version);
      Directory.Delete(path, true);
    }

    private void DeleteExe(string version, string exeName)
    {
      var path = Path.Combine(settingsPathBuilder.GetLlvmPath(version), exeName);
      File.Delete(path);
    }

    private void SetUninstallCommandState()
    {
      canUseCommand = true;
      selectedLlvm.IsInstalled = false;
      selectedLlvm.IsDownloading = false;
    }

    private void CreateVersionFolder(string version)
    {
      var path = settingsPathBuilder.GetLlvmPath(version);
      Directory.CreateDirectory(path);
    }

    private bool IsVersionExeOnDisk(string version, string name)
    {
      var executablePath = settingsPathBuilder.GetLlvmExecutablePath(version, name);
      return File.Exists(executablePath);
    }
    #endregion
  }
}
