﻿using System;
using System.Collections.Generic;

namespace ClangPowerTools
{
  [Serializable]
  public class ClangTidyOptions
  {
    #region Properties

    public List<string> TidyChecks { get; set; } = new List<string>();

    public bool AutoTidyOnSave { get; set; }

    public bool Fix { get; set; }

    public bool FormatAfterTidy { get; set; }

    public string HeaderFilter { get; set; }

    public string TidyMode { get; set; }

    #endregion

  }
}
