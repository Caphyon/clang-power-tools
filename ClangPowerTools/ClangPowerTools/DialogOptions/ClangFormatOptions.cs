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

    #endregion

    #region Format Options

    public string AssumeFilename { get; set; }

    public string FallbackStyle { get; set; }

    public bool SortIncludes { get; set; }

    public string Style { get; set; }

    #endregion

    #endregion

  }
}
