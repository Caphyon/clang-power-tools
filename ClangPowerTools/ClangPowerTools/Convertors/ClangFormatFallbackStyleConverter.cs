using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ClangPowerTools.Convertors
{
  class ClangFormatFallbackStyleConverter : IValueConverter
  {

    public static Dictionary<ClangFormatFallbackStyle?, string> StyleDisplay = new Dictionary<ClangFormatFallbackStyle?, string>()
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
      return StyleDisplay[fallbackStyle];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

  }
}
