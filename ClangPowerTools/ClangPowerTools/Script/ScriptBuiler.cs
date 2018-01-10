using System.Reflection;

namespace ClangPowerTools
{
  public abstract class ScriptBuiler
  {
    #region Members

    protected string mParameters = string.Empty;

    #endregion

    #region Methods

    protected string GetScriptPath()
    {
      string assemblyPath = Assembly.GetExecutingAssembly().Location;
      assemblyPath = assemblyPath.Substring(0, assemblyPath.LastIndexOf('\\'));
      return $"{assemblyPath}\\{ScriptConstants.kScriptName}";
    }

    #endregion

  }
}