using ClangPowerTools.Properties;
using ClangPowerTools.TextOperationsInterfaces;
using System.Text.RegularExpressions;

namespace ClangPowerTools
{
  public class ErrorDetector : IDetector
  {

    #region IDetector Implementation


    public bool Detect(string text, out Match matchResult)
    {
      Regex regex = new Regex(ErrorParserConstants.kErrorMessageRegex);
      matchResult = regex.Match(text);
      return matchResult.Success;
    }


    #endregion


    #region Public Methods

    public bool LlvmIsMissing(string aMessages)
    {
      return aMessages.Contains(ErrorParserConstants.kCompileClangMissingFromPath) ||
        aMessages.Contains(ErrorParserConstants.kTidyClangMissingFromPath);
    }

    public bool HasEncodingError(string message)
    {
      return message.Contains(Resources.EncodingError);
    }
    #endregion

  }
}
