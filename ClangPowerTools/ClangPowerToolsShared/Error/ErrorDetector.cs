using ClangPowerTools.MVVM.Constants;
using ClangPowerTools.Properties;
using ClangPowerTools.TextOperationsInterfaces;
using System.Text.RegularExpressions;

namespace ClangPowerTools
{
  public class ErrorDetector : IDetector
  {

    #region IDetector Implementation


    public bool Detect(string text, string pattern, out Match matchResult)
    {
      Regex regex = new Regex(pattern);
      matchResult = regex.Match(text);
      return matchResult.Success;
    }


    #endregion


    #region Public Methods

    public bool HasEncodingError(string message)
    {
      return message.Contains(ResourceConstants.EncodingError);
    }
    #endregion

  }
}
