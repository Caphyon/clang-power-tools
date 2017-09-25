using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClangPowerTools
{
  public class OutputWindowManager
  {
    #region Members

    private DTE2 mDte = null;

    #endregion

    #region Ctor

    public OutputWindowManager(DTE2 aDte) => mDte = aDte;

    #endregion

    #region Public Methods

    public void AddMessage(string aError)
    {
      if (String.IsNullOrWhiteSpace(aError))
        return;

      using (OutputWindow outputWindow = new OutputWindow(mDte))
      {
        outputWindow.Show(mDte);
        outputWindow.Write(aError);
      }
    }

    public void AddMessages(IEnumerable<string> aErrors)
    {
      using (OutputWindow outputWindow = new OutputWindow(mDte))
      {
        outputWindow.Clear();
        outputWindow.Show(mDte);
        foreach (string error in aErrors.Where(err => !String.IsNullOrWhiteSpace(err)))
          outputWindow.Write(error);
      }
    }

    #endregion

  }
}
