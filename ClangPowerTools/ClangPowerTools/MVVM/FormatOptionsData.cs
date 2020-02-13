using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public class FormatOptionsData
  {
    public static List<IFormatOption> FormatOptions = new List<IFormatOption>()
    {
      new FormatOptionModel{ Name = "AccessModifierOffset", Paramater = "int", Description = "The extra indent or outdent of access modifiers, e.g. public:.", Input = "0"},
      new FormatOptionToggleModel{Name = "AlignEscapedNewlinesLeft", Paramater = "bool", Description = "If \"true\", aligns escaped newlines as far left as possible.\r\n\r\nOtherwise puts them into the right-most column. This does not necessarily mean flushing lings to the left but, rather, attempting to align the current line's left margin with the previous line's left margin", IsEnabled=false},
      new FormatOptionToggleModel{Name = "AlignTrailingComments", Paramater = "bool", Description = "If \"true\", aligns trailing comments", IsEnabled=false},
      new FormatOptionToggleModel{Name = "AllowAllParametersOfDeclarationOnNextLine", Paramater = "bool", Description = "If the function declaration doesn’t fit on a line, allow putting all parameters of a function declaration onto the next line even if BinPackParameters is false.", IsEnabled=false},
      new FormatOptionToggleModel{Name = "AllowShortIfStatementsOnASingleLine", Paramater = "bool", Description = "If true, if (a) return; can be put on a single line.", IsEnabled=false},
      new FormatOptionToggleModel{Name = "AllowShortLoopsOnASingleLine", Paramater = "bool", Description = "If true, while (true) continue; can be put on a single line.", IsEnabled=false},
      new FormatOptionToggleModel{Name = "AlwaysBreakBeforeMultilineStrings", Paramater = "bool", Description = "If true, always break before multiline string literals.", IsEnabled=false},
      new FormatOptionToggleModel{Name = "AlwaysBreakTemplateDeclarations", Paramater = "bool", Description = "If true, always break after the template<…> of a template declaration", IsEnabled=false},
      new FormatOptionToggleModel{Name = "BinPackParameters", Paramater = "bool", Description = "If false, a function call’s or function definition’s parameters will either all be on the same line or will have one line each.", IsEnabled=false},
      new FormatOptionToggleModel{Name = "BreakBeforeBinaryOperators", Paramater = "bool", Description = "If true, binary operators will be placed after line breaks.", IsEnabled=false},
      new FormatOptionModel{ Name = "BreakBeforeBraces ", Paramater = "BraceBreakingStyle", Description = "- BS_Attach (in configuration: Attach) Always attach braces to surrounding context.\r\n- BS_Linux (in configuration: Linux) Like Attach, but break before braces on function, namespace and class definitions.\r\n- BS_Stroustrup (in configuration: Stroustrup) Like Attach, but break before function definitions.\r\n- BS_Allman (in configuration: Allman) Always break before braces.", Input = "Allman"},
      new FormatOptionToggleModel{Name = "BreakBeforeTernaryOperators", Paramater = "bool", Description = "If true, ternary operators will be placed after line breaks.", IsEnabled=false},
      new FormatOptionToggleModel{Name = "BreakConstructorInitializersBeforeComma", Paramater = "bool", Description = "Always break constructor initializers before commas and align the commas with the colon.", IsEnabled=false}
    };
  }
}
