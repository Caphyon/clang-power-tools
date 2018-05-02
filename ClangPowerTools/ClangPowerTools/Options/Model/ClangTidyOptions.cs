﻿using System;
using System.Collections.Generic;

namespace ClangPowerTools
{
  [Serializable]
  public class ClangTidyOptions
  {
    #region Properties

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public List<string> TidyChecks { get; set; } = new List<string>();

    public string TidyChecksCollection { get; set; }


    public bool AutoTidyOnSave { get; set; }

    public bool Fix { get; set; }

    public bool FormatAfterTidy { get; set; }

    public ClangTidyHeaderFilters? HeaderFilter { get; set; }

    public ClangTidyUseChecksFrom? TidyMode { get; set; }

    #endregion

  }
}
