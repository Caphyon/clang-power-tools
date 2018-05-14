using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ClangPowerTools.Convertors
{
  public class ClangFormatFallbackStyleConverter : IValueConverter
  {
    private static Dictionary<ClangFormatFallbackStyle?, string> fallbackStyleDisplay = 
      new Dictionary<ClangFormatFallbackStyle?, string>()
      {
        { ClangFormatFallbackStyle.none, "none" },
        { ClangFormatFallbackStyle.file, "file" },
        { ClangFormatFallbackStyle.Chromium, "Chromium" },
        { ClangFormatFallbackStyle.Google, "Google" },
        { ClangFormatFallbackStyle.LLVM, "LLVM" },
        { ClangFormatFallbackStyle.Mozilla, "Mozilla" },
        { ClangFormatFallbackStyle.WebKit, "WebKit" },
      };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      ClangFormatFallbackStyle fallbackStyle = (ClangFormatFallbackStyle)value;
      return fallbackStyleDisplay[fallbackStyle];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

  }
}
