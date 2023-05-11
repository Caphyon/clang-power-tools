using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Constants;
using System;
using System.IO;
using System.Net;
using System.Text;
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
      string scriptUri = PsUpdaterConstants.GitHubUri + PsUpdaterConstants.ClangBuildScript;
      await DownloadScriptAsync(scriptUri, PsUpdaterConstants.ClangBuildScript);
      ReplaceScripts();
    }

    #endregion

    #region Private Methods

    private async Task DownloadScriptAsync(string fileUri, string fileName)
    {
      string scriptsDirectory = settingsPathBuilder.GetPath(PsUpdaterConstants.PowerShellScriptsFolder);
      string scriptFullName = Path.Combine(scriptsDirectory, fileName);
      try
      {
        FileSystem.CreateDirectory(scriptsDirectory);
        using WebClient client = new();
        await client.DownloadFileTaskAsync(new Uri(fileUri), scriptFullName);
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "PowerShell Scripts", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
    }

    private void ReplaceScripts()
    {
      string destFolder = Path.GetDirectoryName(settingsPathBuilder.GetAssemblyLocalPath());
      destFolder = Path.Combine(destFolder, PsUpdaterConstants.ToolingFolder, PsUpdaterConstants.V1Folder);
      string sourceFolder = settingsPathBuilder.GetPath(PsUpdaterConstants.PowerShellScriptsFolder);

      try
      {
        //Check if file was downloaded, delete destination folder, all files will be automatically downloaded
        //If file wasn't download, an exception will throw on MoveFile action
        FileInfo sourceFile = new FileInfo(Path.Combine(sourceFolder, PsUpdaterConstants.ClangBuildScript));
        if (File.Exists(sourceFile.FullName) && sourceFile.Length > 0)
        {
          FileSystem.DeleteDirectory(destFolder);
        }
        else {
          MessageBox.Show("The download of clang-build.ps1 cannot be completed due to a potential issue with your internet connection. Please verify your connectivity.", "PowerShell Scripts", MessageBoxButtons.OK, MessageBoxIcon.Information);
          return;
        }
        
        Directory.CreateDirectory(destFolder);
        FileSystem.MoveFile(Path.Combine(sourceFolder, PsUpdaterConstants.ClangBuildScript),
                            Path.Combine(destFolder, PsUpdaterConstants.ClangBuildScript));

        destFolder = Path.Combine(destFolder, PsUpdaterConstants.PsClangFolder);
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "PowerShell Scripts", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }

      ClearFilesAfterReplace();
      MessageBox.Show("PowerShell scripts were updated to the latest version.", "PowerShell Scripts", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ClearFilesAfterReplace()
    {
      string scriptsDirectory = settingsPathBuilder.GetPath(PsUpdaterConstants.PowerShellScriptsFolder);
      try
      {
        FileSystem.DeleteDirectory(scriptsDirectory);
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "PowerShell Scripts", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    #endregion
  }
}
