using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ClangPowerTools.Convertors
{
  public class ClangStyleConverter : IValueConverter
  {

    public static Dictionary<ClangStyle?, string> StyleDisplay = new Dictionary<ClangStyle?, string>()
    {
      { ClangStyle.File, "file" },
      { ClangStyle.Chromium, "Chromium" },
      { ClangStyle.Google, "Google" },
      { ClangStyle.LLVM, "LLVM" },
      { ClangStyle.Mozilla, "Mozilla" },
      { ClangStyle.WebKit, "WebKit" },
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      ClangStyle style = (ClangStyle)value;
      return StyleDisplay[style];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
