using ClangPowerTools.TextOperationsInterfaces;
using System.Text.RegularExpressions;

namespace ClangPowerTools.Error
{
  class ErrorFormatter : ITextFormatter
  {

    #region ITextFormater Implementation

    public string Format(string aText, string aReplacement)
    {
      Regex regex = new Regex(ErrorParserConstants.kErrorMessageRegex);
      return regex.Replace(aText, aReplacement, 1);
    }

    #endregion

  }
}
