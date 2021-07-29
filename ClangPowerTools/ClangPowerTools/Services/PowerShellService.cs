using ClangPowerTools.MVVM.Constants;
using System;
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace ClangPowerTools.Services
{
  public class PowerShellService
  {
    #region Members

    public CancellationTokenSource downloadCancellationToken = new();

    private readonly SettingsPathBuilder settingsPathBuilder = new();

    #endregion


    #region Public Methods

    public void UpdateScripts()
    {
      DownloadScript();
    }

    #endregion


    #region Private Methods

    private void DownloadScript(string fileUri, string fileName)
    {
      var appDataFolder = settingsPathBuilder.GetPath("clang-build.ps1");
      try
      {
        using WebClient client = new();
        client.DownloadFileCompleted += DownloadCompleted;
        downloadCancellationToken.Token.Register(client.CancelAsync);
        client.DownloadFileAsync(new Uri(PowerShellConstants.ClangBuildScriptUri), appDataFolder);
      }
      catch (Exception)
      {
        DownloadCanceled();
      }
    }

    private void DownloadCanceled()
    {
      //OnOperationCanceldEvent();

      MessageBox.Show("The download process has stopped.", "PowerShell script udpate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
    {
      if (downloadCancellationToken.IsCancellationRequested)
      {
        DownloadCanceled();
      }
      else
      {
        ReplaceScripts();
      }
    }


    private void ReplaceScripts()
    {

    }

    private void ClearFilesAfterReplace()
    {

    }

    #endregion
  }
}
