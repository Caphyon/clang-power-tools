using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ClangPowerTools.Convertors
{
  public class ClangTidyHeaderFiltersConvertor : IValueConverter
  {
    #region Members 

    private static readonly Dictionary<ClangTidyHeaderFilters?, string> mTidyHeaderFiltersEnumToString =
      new Dictionary<ClangTidyHeaderFilters?, string>
      {
            {ClangTidyHeaderFilters.DefaultHeaderFilter, ComboBoxConstants.kDefaultHeaderFilter},
            {ClangTidyHeaderFilters.CorrespondingHeader, ComboBoxConstants.kCorrespondingHeader }
      };

    private static readonly Dictionary<string, ClangTidyHeaderFilters?> mAdditionalIncludesStringToEnum =
      new Dictionary<string, ClangTidyHeaderFilters?>
      {
        { ComboBoxConstants.kDefaultHeaderFilter, ClangTidyHeaderFilters.DefaultHeaderFilter },
        { ComboBoxConstants.kCorrespondingHeader, ClangTidyHeaderFilters.CorrespondingHeader }
      };


    public static readonly Dictionary<string, string> mHeaderFilterScriptMapingEnumToString =
      new Dictionary<string, string>
    {
      { ComboBoxConstants.kCorrespondingHeader, "_" },
    };

    public static readonly Dictionary<string, string> mHeaderFilterScriptMapingStringToEnum =
      new Dictionary<string, string>
      {
            {"_", ComboBoxConstants.kCorrespondingHeader }
      };

    #endregion

    #region Public Methods

    #region To and From String

    public static string ToString(ClangTidyHeaderFilters? aHeaderFilters)
      => mTidyHeaderFiltersEnumToString[aHeaderFilters];

    public static ClangTidyHeaderFilters? FromString(string aHeaderFilters)
      => mAdditionalIncludesStringToEnum[aHeaderFilters];

    #endregion


    #region Script Maping

    public static string ScriptMaping(ClangTidyHeaderFilters? aHeaderFilters)
     => mHeaderFilterScriptMapingEnumToString[mTidyHeaderFiltersEnumToString[aHeaderFilters]];

    public static bool ScriptMapingContainsKey(ClangTidyHeaderFilters? aHeaderFilters)
      => mHeaderFilterScriptMapingEnumToString.ContainsKey(mTidyHeaderFiltersEnumToString[aHeaderFilters]);

    #endregion


    #region IValueConverter Implementation

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      ClangTidyHeaderFilters aHeaderFilters = (ClangTidyHeaderFilters)value;
      return mTidyHeaderFiltersEnumToString[aHeaderFilters];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion



    #endregion

  }
}
