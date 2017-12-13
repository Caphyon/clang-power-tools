using System;
using System.Collections.Generic;

namespace ClangPowerTools
{
  [Serializable]
  public class ClangTidyOptions
  {
    #region Members

    private List<string> mTidyChecks = new List<string>();

    #endregion

    #region Properties

    public List<string> TidyChecks { get; set; } = new List<string>();

    public bool Fix { get; set; }

    public string TidyModes { get; set; }

    #endregion

  }
}
