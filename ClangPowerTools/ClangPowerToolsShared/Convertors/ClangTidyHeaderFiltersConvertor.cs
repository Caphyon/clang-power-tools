using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ClangPowerTools.Convertors
{
  public class ClangTidyHeaderFiltersConvertor : IValueConverter
  {
    #region Members 

    private static Dictionary<string, string> mCorrespondingHeaderScriptEncode =
      new Dictionary<string, string>()
      {
        {ComboBoxConstants.kDefaultHeaderFilter, ComboBoxConstants.kDefaultHeaderFilter },
        {ComboBoxConstants.kCorrespondingHeaderName, ComboBoxConstants.kCorrespondingHeaderValue }
      };

    private static Dictionary<string, string> mCorrespondingHeaderScriptDecode =
      new Dictionary<string, string>()
      {
        {ComboBoxConstants.kDefaultHeaderFilter, ComboBoxConstants.kDefaultHeaderFilter },
        {ComboBoxConstants.kCorrespondingHeaderValue, ComboBoxConstants.kCorrespondingHeaderName }
      };

    #endregion

    #region Public Methods

    public static string ScriptEncode(string aHeaderFilters)
    {
      if (false == mCorrespondingHeaderScriptEncode.ContainsKey(aHeaderFilters))
        return string.Empty;

      return mCorrespondingHeaderScriptEncode[aHeaderFilters];
    }

    public static string ScriptDecode(string aHeaderFilters)
    {
      if (false == mCorrespondingHeaderScriptDecode.ContainsKey(aHeaderFilters))
        return string.Empty;

      return mCorrespondingHeaderScriptDecode[aHeaderFilters];
    }

    #region IValueConverter Implementation

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return ((HeaderFiltersValue)value).HeaderFilters;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return new HeaderFiltersValue(value.ToString());
    }

    #endregion

    #endregion

  }
}


