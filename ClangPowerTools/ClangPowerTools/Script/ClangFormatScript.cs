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

      if (false == string.IsNullOrWhiteSpace(aClangFormatPage.AssumeFilename))
        mParameters = $"{mParameters} {ScriptConstants.kAssumeFilename}={aClangFormatPage.AssumeFilename}";

      if (false == string.IsNullOrWhiteSpace(aClangFormatPage.FallbackStyle))
        mParameters = $"{mParameters} {ScriptConstants.kFallbackStyle}={aClangFormatPage.FallbackStyle}";

      //if (aClangFormatPage.SortIncludes)
      //  mParameters = $"{mParameters} {ScriptConstants.kSortIncludes}";

      if (false == string.IsNullOrWhiteSpace(aClangFormatPage.Style))
        mParameters = $"{mParameters} {ScriptConstants.kStyle}={aClangFormatPage.Style}";
    }

    #region Get Parameters Helpers

    //Get the clang-format.exe path
    protected override string GetFilePath() => Path.Combine(base.GetFilePath(), ScriptConstants.kClangFormat);

    #endregion
  }
}
