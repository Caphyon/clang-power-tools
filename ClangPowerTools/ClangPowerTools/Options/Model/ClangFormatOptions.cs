using ClangPowerTools.Options;
using System;

namespace ClangPowerTools
{
  [Serializable]
  public class ClangFormatOptions
  {
    #region Properties 


    #region Format On Save

    public bool EnableFormatOnSave { get; set; }

    public string FileExtensions { get; set; }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public string SkipFiles { get; set; }

    public ClangFormatSkipValue SkipFilesValue { get; set; }


    #endregion


    #region Format Options

    public string AssumeFilename { get; set; }

    public ClangFormatFallbackStyle? FallbackStyle { get; set; }

    //public bool SortIncludes { get; set; }

    public ClangFormatStyle? Style { get; set; }


    #endregion


    #region Clang-Format executable path

    public ClangFormatPathValue ClangFormatPath { get; set; }

    #endregion


    #endregion

  }
}
