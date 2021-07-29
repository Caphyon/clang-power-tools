using System;
using System.IO;

namespace ClangPowerTools
{
  public class SettingsPathBuilder
  {
    #region Constants

    private const string folderName = "ClangPowerTools";
    private const string llvm = "LLVM";
    private const string binFolder = "bin";
    private const string fileExtension = ".exe";

    #endregion

    #region Methods

    public string GetPath(string aFileName) => Path.Combine(GetFolderPath(), aFileName);

    public string GetLlvmPath(string version)
    {
      var appdDataPath = GetPath("");
      var folderName = string.Concat(llvm, version);
      return Path.Combine(appdDataPath, llvm, folderName);
    }

    public string GetLlvmBinPath(string version)
    {
      var path = GetLlvmPath(version);
      return Path.Combine(path, binFolder);
    }

    public string GetLlvmExecutablePath(string version, string executableName)
    {
      var path = GetLlvmPath(version);
      var executable = string.Concat(executableName, fileExtension);
      return Path.Combine(path, executable);
    }

    private string GetFolderPath()
    {
      string folderPath = Path.Combine
        (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), folderName);

      if (!Directory.Exists(folderPath))
        Directory.CreateDirectory(folderPath);

      return folderPath;
    }

    #endregion

  }
}
