using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public class FormatOptionsData
  {
    public static List<IFormatOption> FormatOptions = new List<IFormatOption>()
    {
      new FormatOptionModel{ Name = "AccessModifierOffset", Paramater = "int", Description = "The extra indent or outdent of access modifiers, e.g. public:.", Input = "InputTest"},
      new FormatOptionToggleModel{Name = "toggleTest", Paramater = "bool", Description = "yyyyy", IsEnabled=false}
    };
  }
}
