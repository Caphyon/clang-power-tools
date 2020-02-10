using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;

namespace ClangPowerTools
{
  public class FormatOptionsData
  {
    public static List<IFormatOption> FormatOptions = new List<IFormatOption>()
    {
      new FormatOptionModel{ Name = "AccessModifierOffset", Paramater = "int", Description = "The extra indent or outdent of access modifiers, e.g. public:.", Input = string.Empty},
      new FormatOptionToggleModel{Name = "AlignEscapedNewlinesLeft", Paramater = "bool", Description = "If true, aligns escaped newlines as far left as possible. Otherwise puts them into the right-most column. This does not necessarily mean flushing lings to the left but, rather, attempting to align the current line's left margin with the previous line's left margin", IsEnabled=false},
      new FormatOptionToggleModel{Name = "AlignTrailingComments", Paramater = "bool", Description = "If <Run Background=\"#FFCDCDCD\" Text=\"true\"/>, aligns trailing comments", IsEnabled=false}
    };
  }
}
