using ClangPowerTools.DialogPages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.Script
{
  public class ClangFormatScript : ScriptBuiler
  {

    public string GetScript(IItem aItem, string aSolutionPath)
    {
      return $"{ScriptConstants.kScriptBeginning} ''{GetFilePath()}'' {mParameters}'";
    }

    public void ConstructParameters(ClangFormatPage aClangFormatPage)
    {
      mParameters = string.Empty;

      if (null != aClangFormatPage.AssumeFilename && string.IsNullOrWhiteSpace(aClangFormatPage.AssumeFilename))
        mParameters = $"{mParameters} {ScriptConstants.kFallbackStyle}={aClangFormatPage.AssumeFilename}";

      if (null != aClangFormatPage.FallbackStyle && string.IsNullOrWhiteSpace(aClangFormatPage.FallbackStyle))
        mParameters = $"{mParameters} {ScriptConstants.kFallbackStyle}={aClangFormatPage.FallbackStyle}";

      if (aClangFormatPage.SortIncludes)
        mParameters = $"{mParameters} {ScriptConstants.kSortIncludes}";

      if (null != aClangFormatPage.Style && string.IsNullOrWhiteSpace(aClangFormatPage.Style))
        mParameters = $"{mParameters} {ScriptConstants.kStyle}={aClangFormatPage.Style}";
    }

    #region Get Parameters Helpers

    //Get the clang-format.exe path
    protected override string GetFilePath() => Path.Combine(base.GetFilePath(), ScriptConstants.kClangFormat);

    #endregion
  }
}
