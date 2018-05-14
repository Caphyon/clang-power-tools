using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ClangPowerTools.Convertors
{
  public class ClangTidyUseChecksFromConvertor : IValueConverter
  {
    #region Members

    private static readonly Dictionary<ClangTidyUseChecksFrom?, string> mClangTidyUseChecksFromEnumToString =
      new Dictionary<ClangTidyUseChecksFrom?, string>
      {
            {ClangTidyUseChecksFrom.PredefinedChecks, ComboBoxConstants.kPredefinedChecks},
            {ClangTidyUseChecksFrom.CustomChecks, ComboBoxConstants.kCustomChecks },
            {ClangTidyUseChecksFrom.TidyFile, ComboBoxConstants.kTidyFile }
      };

    private static readonly Dictionary<string, ClangTidyUseChecksFrom?> mClangTidyUseChecksFromStringToEnum =
      new Dictionary<string, ClangTidyUseChecksFrom?>
      {
             {ComboBoxConstants.kPredefinedChecks, ClangTidyUseChecksFrom.PredefinedChecks},
             {ComboBoxConstants.kCustomChecks , ClangTidyUseChecksFrom.CustomChecks},
             {ComboBoxConstants.kTidyFile,  ClangTidyUseChecksFrom.TidyFile }
      };

    #endregion


    #region Publci Methods

    public static string ToString(ClangTidyUseChecksFrom? aChecksFrom)
      => mClangTidyUseChecksFromEnumToString[aChecksFrom];

    public static ClangTidyUseChecksFrom? FromString(string aChecksFrom)
      => mClangTidyUseChecksFromStringToEnum[aChecksFrom];

    #endregion


    #region IValueConverter Implementation

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      ClangTidyUseChecksFrom checksFrom = (ClangTidyUseChecksFrom)value;
      return ToString(checksFrom);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion

  }
}
