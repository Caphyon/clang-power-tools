using ClangPowerTools.DialogPages;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ClangPowerTools
{
  public abstract class ScriptBuiler
  {
    #region Members

    protected string mParameters = string.Empty;
    private bool mUseTidyFile;

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