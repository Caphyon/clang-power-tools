using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ClangPowerTools
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

    #endregion

    #region Public Methods

    public static string ToString(ClangTidyHeaderFilters? aHeaderFilters)
      => mTidyHeaderFiltersEnumToString[aHeaderFilters];

    public static ClangTidyHeaderFilters? FromString(string aHeaderFilters)
      => mAdditionalIncludesStringToEnum[aHeaderFilters];


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

  }
}
