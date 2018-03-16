using System.Globalization;

namespace ClangPowerTools
{
  public static class StringExtension
  {
    #region Public Methods

    public static string SubstringAfter(this string source, string value)
    {
      if (string.IsNullOrEmpty(value))
        return source;

      CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
      int index = compareInfo.IndexOf(source, value, CompareOptions.Ordinal);

      if (index < 0)
        return string.Empty; //No such substring

      return source.Substring(index + value.Length);
    }

    public static string SubstringBefore(this string source, string value)
    {
      if (string.IsNullOrEmpty(value))
        return value;

      CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
      int index = compareInfo.IndexOf(source, value, CompareOptions.Ordinal);

      if (index < 0)
        return string.Empty; //No such substring

      return source.Substring(0, index);
    }

    #endregion

  }
}
