using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public class FormatOptions
  {
    public static List<IFormatOption> FormatOptionsList = new List<IFormatOption>()
    {
      new FormatOptionModel{ Name = "test", Description = "xxxxxxxx", Input = "InputTest"},
      new FormatOptionToggleModel{Name = "toggleTest", Description = "yyyyy", IsEnabled=false}
    };
  }
}
