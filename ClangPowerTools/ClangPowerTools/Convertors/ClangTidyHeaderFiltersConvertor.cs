using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ClangPowerTools.Convertors
{
  public class ClangTidyHeaderFiltersConvertor : IValueConverter
  {
    #region Members 

    private static readonly Dictionary<ClangTidyHeaderFilters?, string> mTidyHeaderFiltersEnumToStringName =
      new Dictionary<ClangTidyHeaderFilters?, string>
      {
            {ClangTidyHeaderFilters.DefaultHeaderFilter, ComboBoxConstants.kDefaultHeaderFilter},
            {ClangTidyHeaderFilters.CorrespondingHeader, ComboBoxConstants.kCorrespondingHeaderName }
      };

    private static readonly Dictionary<ClangTidyHeaderFilters?, string> mTidyHeaderFiltersEnumToStringValue =
      new Dictionary<ClangTidyHeaderFilters?, string>
      {
            {ClangTidyHeaderFilters.DefaultHeaderFilter, ComboBoxConstants.kDefaultHeaderFilter},
            {ClangTidyHeaderFilters.CorrespondingHeader, ComboBoxConstants.kCorrespondingHeaderValue }
      };

    private static readonly Dictionary<string, ClangTidyHeaderFilters?> mAdditionalIncludesStringToEnum =
      new Dictionary<string, ClangTidyHeaderFilters?>
      {
        { ComboBoxConstants.kDefaultHeaderFilter, ClangTidyHeaderFilters.DefaultHeaderFilter },
        { ComboBoxConstants.kCorrespondingHeaderName, ClangTidyHeaderFilters.CorrespondingHeader }
      };


    public static readonly Dictionary<string, string> mHeaderFilterScriptMapingEnumToString =
      new Dictionary<string, string>
    {
      { ComboBoxConstants.kCorrespondingHeaderName, "_" },
    };

    public static readonly Dictionary<string, string> mHeaderFilterScriptMapingStringToEnum =
      new Dictionary<string, string>
      {
            {"_", ComboBoxConstants.kCorrespondingHeaderName }
      };

    #endregion

    #region Public Methods

    #region To and From String

    public static string ToStringName(ClangTidyHeaderFilters? aHeaderFilters)
      => mTidyHeaderFiltersEnumToStringName[aHeaderFilters];

    public static string ToStringValue(ClangTidyHeaderFilters? aHeaderFilters)
      => mTidyHeaderFiltersEnumToStringValue[aHeaderFilters];


    public static ClangTidyHeaderFilters? FromStringName(string aHeaderFilters)
      => mAdditionalIncludesStringToEnum[aHeaderFilters];

    public static ClangTidyHeaderFilters? FromStringValue(string aHeaderFilters)
     => mAdditionalIncludesStringToEnum[aHeaderFilters];

    #endregion


    #region Script Maping

    public static string ScriptMaping(ClangTidyHeaderFilters? aHeaderFilters)
     => mHeaderFilterScriptMapingEnumToString[mTidyHeaderFiltersEnumToStringName[aHeaderFilters]];

    public static bool ScriptMapingContainsKey(ClangTidyHeaderFilters? aHeaderFilters)
      => mHeaderFilterScriptMapingEnumToString.ContainsKey(mTidyHeaderFiltersEnumToStringName[aHeaderFilters]);

    #endregion


    #region IValueConverter Implementation

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      ClangTidyHeaderFilters aHeaderFilters = (ClangTidyHeaderFilters)value;
      return ToStringName(aHeaderFilters);//   mTidyHeaderFiltersEnumToStringName[aHeaderFilters];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion



    #endregion

  }
}
