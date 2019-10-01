using ClangPowerTools.MVVM.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
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

    private Process process;
    private List<LlvmModel> llvms;
    private LlvmModel llvmModel;
    private string executableName = "LLVM";
    private string appdDataPath;
    private ICommand dowloadCommand;
    private ICommand uninstallCommand;
    private ICommand stopCommand;

    #endregion

    #region Constructor
    public LlvmSettingsViewModel()
    {
      llvmModel = new LlvmModel();
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

    public LlvmModel SelectedLlvm
    {
      get
      {
        return llvmModel;
      }
      set
      {
        llvmModel = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedLlvm"));
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
    public ICommand DownloadCommand
    {
      get => dowloadCommand ?? (dowloadCommand = new RelayCommand(() => DownloadLlvmVersion(SelectedLlvm.Version), () => CanExecute));
    }

    public ICommand UninstallCommand
    {
      get => uninstallCommand ?? (uninstallCommand = new RelayCommand(() => DeleteLlvmVersion("8.0.0"), () => CanExecute));
    }

    public ICommand StopCommand
    {
      get => stopCommand ?? (stopCommand = new RelayCommand(() => StopDownload(), () => CanExecute));
    }


    #endregion

    #region Methods
    private void DownloadLlvmVersion(string version)
    {
      CreateVersionFolder(version);

      var executablePath = string.Concat(GetLlVmVersionPath(version), "\\", executableName, version, ".exe");
      var finalUri = string.Concat(uri, "/", version, "/", executableName, "-", version, "-win64.exe");

      //using (var client = new WebClient())
      //{
      //  client.DownloadProgressChanged += DownloadProgressChanged;
      //  client.DownloadFileCompleted += DownloadFileCompleted;
      //  client.DownloadFileAsync(new Uri(finalUri), executablePath);
      //}
    }

    private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
      InstallLlVmVersion(SelectedLlvm.Version);
    }

    private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      llvmModel.DownloadProgress = e.ProgressPercentage;
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

    }

    private void StopDownload()
    {

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
      var path = Path.Combine(appdDataPath, version);
      if (Directory.Exists(path)) return true;

      return false;
    }
    #endregion
  }
}
