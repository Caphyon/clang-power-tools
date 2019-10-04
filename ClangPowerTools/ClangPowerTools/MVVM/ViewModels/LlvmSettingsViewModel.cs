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
    private const string executableName = "LLVM";

    private Process process;
    private ObservableCollection<LlvmModel> llvms;
    private LlvmModel selectedLlvm = new LlvmModel();
    private CancellationTokenSource cancellationToken = new CancellationTokenSource();
    private bool canDownload = true;
    private string appdDataPath;
    private ICommand uninstallCommand;
    private ICommand cancelCommand;

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

    public ObservableCollection<LlvmModel> Llvms
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
    public ICommand UninstallCommand
    {
      get => uninstallCommand ?? (uninstallCommand = new RelayCommand(() => DeleteLlvmVersion("8.0.0"), () => CanExecute));
    }

    public ICommand CancelCommand
    {
      get => cancelCommand ?? (cancelCommand = new RelayCommand(() => CancelDownload(), () => CanExecute));
    }


    #endregion

    #region Methods
    public void SetSelectedElement(int elementIndex)
    {
      if (canDownload)
      {
        selectedLlvm = llvms[elementIndex];
        selectedLlvm.IsDownloading = true;
        canDownload = false;
        DownloadLlvmVersion(selectedLlvm.Version);
      }
      else if (selectedLlvm.IsDownloading && selectedLlvm == llvms[elementIndex])
      {
        canDownload = true;
        selectedLlvm.DownloadProgress = 0;
        selectedLlvm.IsDownloading = false;
        CancelDownload();
      }

    }

    private void DownloadLlvmVersion(string version)
    {
      CreateVersionFolder(version);

      var executablePath = string.Concat(GetLlVmVersionPath(version), "\\", executableName, version, ".exe");
      var finalUri = string.Concat(uri, "/", version, "/", executableName, "-", version, "-win64.exe");

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
      var executablePath = GetLlVmVersionPath(version);
      var startInfoArguments = string.Concat(arguments, " ", executablePath, " ", exeParameters);

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
      process.Dispose();
    }


    private void DeleteLlvmVersion(string version)
    {
      var path = GetLlVmVersionPath(version);
      Directory.Delete(path, true);
    }

    private void CancelDownload()
    {
      cancellationToken.Cancel();
      //process.Kill();
      //process.Dispose();
    }

    private string GetLlVmVersionPath(string version)
    {
      var folderName = string.Concat(executableName, version);
      return Path.Combine(appdDataPath, executableName, folderName);
    }

    private void CreateVersionFolder(string version)
    {
      var path = GetLlVmVersionPath(version);
      Directory.CreateDirectory(path);
    }

    private void IntitializeView()
    {
      llvms = new ObservableCollection<LlvmModel>();

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
      var path = Path.Combine(appdDataPath, version);
      if (Directory.Exists(path)) return true;

      return false;
    }
    #endregion
  }
}
