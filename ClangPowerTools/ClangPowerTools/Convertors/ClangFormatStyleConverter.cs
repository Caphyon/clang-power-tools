using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ClangPowerTools.Convertors
{
  public class ClangFormatStyleConverter : IValueConverter
  {

    public static Dictionary<ClangFormatStyle?, string> styleDisplay = new Dictionary<ClangFormatStyle?, string>()
    {
      { ClangFormatStyle.file, "file" },
      { ClangFormatStyle.Chromium, "Chromium" },
      { ClangFormatStyle.Google, "Google" },
      { ClangFormatStyle.LLVM, "LLVM" },
      { ClangFormatStyle.Mozilla, "Mozilla" },
      { ClangFormatStyle.WebKit, "WebKit" },
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      ClangFormatStyle style = (ClangFormatStyle)value;
      return styleDisplay[style];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
