using ClangPowerTools.MVVM.Constants;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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

    public async Task UpdateScriptsAsync()
    {
      await DownloadScriptAsync(PowerShellConstants.ClangBuildScriptUri, PowerShellConstants.ClangBuildScriptName);
    }

    #endregion


    #region Private Methods


    private async Task DownloadScriptAsync(string fileUri, string fileName)
    {
      string scriptFullName = settingsPathBuilder.GetPath(fileName);
      try
      {
        using WebClient client = new();
        client.DownloadFileCompleted += DownloadCompleted;
        downloadCancellationToken.Token.Register(client.CancelAsync);
        await client.DownloadFileTaskAsync(new Uri(fileUri), scriptFullName);
      }
      catch (Exception)
      {
        DownloadCanceled();
      }
    }

    private void DownloadCanceled()
    {
      //OnOperationCanceldEvent();

      MessageBox.Show("The download process has stopped.", "PowerShell Scripts", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
      string installationPath = Path.GetDirectoryName(settingsPathBuilder.GetAssemblyLocalPath());
      MessageBox.Show("PowerShell scripts are updated to the latest version.", "PowerShell Scripts", MessageBoxButtons.OK, MessageBoxIcon.Information);

    }

    private void ClearFilesAfterReplace()
    {

    }

    #endregion
  }
}
