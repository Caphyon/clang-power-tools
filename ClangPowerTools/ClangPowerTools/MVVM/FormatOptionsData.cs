using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public class FormatOptionsData
  {
    public static List<IFormatOption> FormatOptions = new List<IFormatOption>()
    {
      new FormatOptionModel{ Name = "test", Description = "xxxxxxxx", Input = "InputTest"},
      new FormatOptionToggleModel{Name = "toggleTest", Description = "yyyyy", IsEnabled=false}
    };
  }
}
