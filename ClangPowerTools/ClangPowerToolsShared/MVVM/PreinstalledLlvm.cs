using ClangPowerTools.MVVM.Controllers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ClangPowerTools
{
  public class PreinstalledLlvm
  {
    #region Members

    private readonly List<LlvmModel> llvms;
    private readonly ObservableCollection<string> installedLlvms;

    private readonly LlvmController llvmController = new LlvmController();

    #endregion

    public PreinstalledLlvm(List<LlvmModel> llvms, ObservableCollection<string> installedLlvms)
    {
      this.llvms = llvms;
      this.installedLlvms = installedLlvms;
    }

    #region Methods

    public void SetPreinstalledLlvm(string path = null, string version = null)
    {
      var llvmSettingsModel = SettingsProvider.LlvmSettingsModel;

      if (path == null || version == null)
        GetPathAndVersion(out path, out version);

      if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(version))
        return;

      SetPathAndVersion(path, version);
      if (llvmSettingsModel.PreinstalledLlvmVersion == string.Empty)
        return;
      if (!IsVersionInstalled(version))
        installedLlvms.Add(llvmSettingsModel.PreinstalledLlvmVersion);
    }

    private bool IsVersionInstalled(string version)
    {
      foreach (var llvm in installedLlvms)
      {
        if (llvm == version)
        {
          return true;
        }
      }
      return false;
    }

    private void SetPathAndVersion(string path, string version)
    {
      var settingsProviderLlvmModel = SettingsProvider.LlvmSettingsModel;
      if (string.IsNullOrWhiteSpace(settingsProviderLlvmModel.PreinstalledLlvmVersion) || 
        string.IsNullOrWhiteSpace(settingsProviderLlvmModel.PreinstalledLlvmPath) ||
        (!string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(version) && 
        version != settingsProviderLlvmModel.PreinstalledLlvmVersion))
      {
        settingsProviderLlvmModel.PreinstalledLlvmVersion = version;
        settingsProviderLlvmModel.PreinstalledLlvmPath = path;
      }

      if (Directory.Exists(settingsProviderLlvmModel.PreinstalledLlvmPath) == false)
      {
        if (settingsProviderLlvmModel.PreinstalledLlvmVersion
          == settingsProviderLlvmModel.LlvmSelectedVersion)
        {
          settingsProviderLlvmModel.LlvmSelectedVersion = string.Empty;
        }

        settingsProviderLlvmModel.PreinstalledLlvmPath = string.Empty;
        settingsProviderLlvmModel.PreinstalledLlvmVersion = string.Empty;
      }
    }

    private void GetPathAndVersion(out string path, out string version)
    {
      path = string.Empty;
      version = string.Empty;
      var llvmSettingsModel = SettingsProvider.LlvmSettingsModel;
      if (installedLlvms.Count == 0)
      {
        path = llvmController.GetLlvmPathFromRegistry();
        version = llvmController.GetVersionFromRegistry();
        llvmSettingsModel.LlvmSelectedVersion = version;
      }
      if (path == string.Empty || version == string.Empty)
      {
        path = llvmSettingsModel.PreinstalledLlvmPath;
        version = llvmSettingsModel.PreinstalledLlvmVersion;
      }
    }

    #endregion

  }
}
