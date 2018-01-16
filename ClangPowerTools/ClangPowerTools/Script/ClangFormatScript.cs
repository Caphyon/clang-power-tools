using ClangPowerTools.DialogPages;
using System.IO;

namespace ClangPowerTools.Script
{
  public class ClangFormatScript : ScriptBuiler
  {
    public string GetScript(string aFilePath)
    {
      return $"{ScriptConstants.kScriptBeginning} ''{GetFilePath()}'' -i {mParameters} ''{aFilePath}'''";
    }

    public void ConstructParameters(ClangFormatPage aClangFormatPage)
    {
      mParameters = string.Empty;

      if (null != aClangFormatPage.AssumeFilename && !string.IsNullOrWhiteSpace(aClangFormatPage.AssumeFilename))
        mParameters = $"{mParameters} {ScriptConstants.kFallbackStyle}={aClangFormatPage.AssumeFilename}";

      if (null != aClangFormatPage.FallbackStyle && !string.IsNullOrWhiteSpace(aClangFormatPage.FallbackStyle)
        && aClangFormatPage.FallbackStyle != ComboBoxConstants.kNone)
        mParameters = $"{mParameters} {ScriptConstants.kFallbackStyle}={aClangFormatPage.FallbackStyle}";

      if (aClangFormatPage.SortIncludes)
        mParameters = $"{mParameters} {ScriptConstants.kSortIncludes}";

      if (null != aClangFormatPage.Style && !string.IsNullOrWhiteSpace(aClangFormatPage.Style))
        mParameters = $"{mParameters} {ScriptConstants.kStyle}={aClangFormatPage.Style}";
    }

    #region Get Parameters Helpers

    //Get the clang-format.exe path
    protected override string GetFilePath() => Path.Combine(base.GetFilePath(), ScriptConstants.kClangFormat);

    #endregion
  }
}
