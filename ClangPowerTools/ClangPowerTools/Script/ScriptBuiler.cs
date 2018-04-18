using System.Reflection;

namespace ClangPowerTools
{
  public abstract class ScriptBuiler
  {
    #region Members

    protected string mParameters = string.Empty;

    #endregion

    #region Methods

    //return the path of the folder where is located the script file or clang-format.exe
    protected virtual string GetFilePath()
    {
      string assemblyPath = Assembly.GetExecutingAssembly().Location;
      return assemblyPath.Substring(0, assemblyPath.LastIndexOf('\\'));
    }

    #endregion

  }
}