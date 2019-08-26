using System;
using System.Globalization;

namespace ClangPowerTools
{
  public static class StringExtension
  {
    #region Public Methods

    public static string SubstringAfter(this string aText, string aSearchedSubstring)
    {
      if (string.IsNullOrEmpty(aSearchedSubstring))
        return aText;

      CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
      int index = compareInfo.IndexOf(aText, aSearchedSubstring, CompareOptions.Ordinal);

      if (index < 0)
        return string.Empty; //No such substring

      return aText.Substring(index + aSearchedSubstring.Length);
    }

    public static string SubstringBefore(this string aText, string aSearchedSubstring)
    {
      if (string.IsNullOrEmpty(aSearchedSubstring))
        return aSearchedSubstring;

      CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
      int index = compareInfo.IndexOf(aText, aSearchedSubstring, CompareOptions.Ordinal);

      if (index < 0)
        return string.Empty; //No such substring

      return aText.Substring(0, index);
    }

    /// <summary>
    /// String comparison using IndexOf
    /// </summary>
    /// <param name="paragrah"></param>
    /// <param name="word"></param>
    /// <param name="comp"></param>
    /// <returns></returns>
    public static bool Contains(this string paragrah, string word, StringComparison comp)
    {
      return paragrah?.IndexOf(word, comp) >= 0;
    }

    #endregion

  }
}
