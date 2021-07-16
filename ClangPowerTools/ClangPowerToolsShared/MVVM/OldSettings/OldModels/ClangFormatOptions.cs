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

    public string SkipFiles { get; set; }

    #endregion


    #region Format Options

    public string AssumeFilename { get; set; }

    public ClangFormatFallbackStyle FallbackStyle { get; set; }


    public ClangFormatStyle Style { get; set; }


    #endregion


    #region Clang-Format executable path

    public ClangFormatPathValue ClangFormatPath { get; set; }

    #endregion


    #endregion

  }
}
