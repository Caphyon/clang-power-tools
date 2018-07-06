using ClangPowerTools.TextOperationsInterfaces;
using System.Text.RegularExpressions;

namespace ClangPowerTools
{
  public class ErrorDetector : IDetector
  {

    #region IDetector Implementation


    public bool Detect(string aText, out Match aMatchResult)
    {
      Regex regex = new Regex(ErrorParserConstants.kErrorMessageRegex);
      aMatchResult = regex.Match(aText);
      return aMatchResult.Success;
    }


    #endregion


    #region Public Methods


    public bool LlvmIsMissing(string aMessages)
    {
      return aMessages.Contains(ErrorParserConstants.kCompileClangMissingFromPath) ||
        aMessages.Contains(ErrorParserConstants.kTidyClangMissingFromPath);
    }

    #endregion

  }
}
