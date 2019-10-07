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

    private const string exeParameters = "/S /D=";
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

    public bool CanExecute
    {
      get
      {
        return true;
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
      if (canDownload == false && selectedLlvm.IsDownloading)
      {
        canDownload = true;
        selectedLlvm.DownloadProgress = 0;
        selectedLlvm.IsDownloading = false;
        CancelDownload();
      }
    }

    public void UninstallCommand(int elementIndex)
    {
      selectedLlvm = llvms[elementIndex];
      if (canDownload == false && selectedLlvm.IsInstalled)
        UninstallLlvmVersion(selectedLlvm.Version);
    
    }

    #endregion



    #region Methods

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
      if (cancellationToken.IsCancellationRequested == false)
      {
        InstallLlVmVersion(selectedLlvm.Version);
      }
      else if (cancellationToken.IsCancellationRequested == true)
      {
        cancellationToken.Dispose();
        cancellationToken = new CancellationTokenSource();
        DeleteLlvmVersion(selectedLlvm.Version);
      }
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
      var startInfoArguments = string.Concat(arguments, " ", executablePath, " ", exeParameters, llVmVersionPath);

      try
      {
        process = new Process();
        process.StartInfo.FileName = processFileName;
        process.StartInfo.Arguments = startInfoArguments;
        process.StartInfo.Verb = processVerb;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.EnableRaisingEvents = true;
        process.Exited += new EventHandler(ProcessExited);
        process.Start();
      }
      catch (Exception e)
      {

        MessageBox.Show(e.Message, "Installation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void ProcessExited(object sender, EventArgs e)
    {
      selectedLlvm.IsInstalled = true;
      canDownload = true;
      process.Dispose();
    }

    private void UninstallLlvmVersion(string version)
    {
      //uninstall command
    }

    private void DeleteLlvmVersion(string version)
    {
      var path = GetLlVmVersionPath(version);
      Directory.Delete(path, true);
    }

    private void CancelDownload()
    {
      cancellationToken.Cancel();
      process.Kill();
    }

    private string GetLlVmVersionPath(string version)
    {
      var folderName = string.Concat(llvm, version);
      return Path.Combine(appdDataPath, llvm, folderName);
    }

    private string GetLlvmExecutablePath(string version, string executableName)
    {
      var folderName = string.Concat(llvm, version);
      return string.Concat(folderName, "\\", executableName, ".exe");
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


    private bool CheckVersionOnDisk(string version)
    {
      var path = GetLlVmVersionPath(version);
      if (Directory.Exists(path)) return true;

      return false;
    }
    #endregion
  }
}
