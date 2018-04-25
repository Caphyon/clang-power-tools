using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ClangPowerTools.Convertors
{
  public class AdditionalIncludesConvertor : IValueConverter
  {
    #region Members

    private static readonly Dictionary<ClangGeneralAdditionalIncludes?, string> mAdditionalIncludesEnumToString =
      new Dictionary<ClangGeneralAdditionalIncludes?, string>
      {
        {ClangGeneralAdditionalIncludes.IncludeDirectories, ComboBoxConstants.kIncludeDirectories},
        {ClangGeneralAdditionalIncludes.SystemIncludeDirectories, ComboBoxConstants.kSystemIncludeDirectories }
      };

    private static readonly Dictionary<string, ClangGeneralAdditionalIncludes?> mAdditionalIncludesStringToEnum =
      new Dictionary<string, ClangGeneralAdditionalIncludes?>
      {
        { ComboBoxConstants.kIncludeDirectories, ClangGeneralAdditionalIncludes.IncludeDirectories },
        { ComboBoxConstants.kSystemIncludeDirectories, ClangGeneralAdditionalIncludes.SystemIncludeDirectories }
      };

    #endregion

    #region Public Methods

    public static string ToString(ClangGeneralAdditionalIncludes? aAdditionalIncludes) 
      => mAdditionalIncludesEnumToString[aAdditionalIncludes];

    public static ClangGeneralAdditionalIncludes? FromString(string aAdditionalIncludes)
      => mAdditionalIncludesStringToEnum[aAdditionalIncludes];



    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      ClangGeneralAdditionalIncludes additionalIncludes = (ClangGeneralAdditionalIncludes)value;
      return mAdditionalIncludesEnumToString[additionalIncludes];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion

  }
}
