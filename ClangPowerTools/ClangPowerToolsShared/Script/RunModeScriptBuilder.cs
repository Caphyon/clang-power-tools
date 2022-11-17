using ClangPowerTools.Builder;
using System.IO;
using System.Reflection;

namespace ClangPowerTools.Script
{
  public class RunModeScriptBuilder : IBuilder<string>
  {
    #region Members 

    /// <summary>
    /// The resulted script after the build
    /// </summary>
    private string mScript = string.Empty;

    #endregion


    #region Methods

    #region Public Methods

    #region IBuilder Implementation


    /// <summary>
    /// Create the power shell script run mod by setting the execution mode parameters with the main script(clang-build.ps1) file path attached
    /// </summary>
    public void Build()
    {
      mScript = $"{ScriptConstants.kScriptBeginning} '{GetScriptFilePath()}'";
    }


    /// <summary>
    /// Get the power shell script run mode with the main script(clang-build.ps1) file path attached
    /// </summary>
    /// <returns>PowerShell script run mode with the script file attached</returns>
    public string GetResult() => mScript;


    #endregion


    #endregion


    #region Private Methods


    /// <summary>
    /// Get the power shell main script(clang-build.ps1) file path
    /// </summary>
    /// <returns>Script file path</returns>
    protected string GetScriptFilePath()
    {
      var assemblyPath = Assembly.GetExecutingAssembly().Location;
      var scriptDirectory = assemblyPath.Substring(0, assemblyPath.LastIndexOf('\\'));

      return Path.Combine(scriptDirectory, ScriptConstants.ToolingV1, ScriptConstants.kScriptName);
    }


    #endregion


    #endregion

  }
}
