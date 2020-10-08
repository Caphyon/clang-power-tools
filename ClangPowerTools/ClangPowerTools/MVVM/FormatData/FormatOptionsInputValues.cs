using System.Collections.Generic;

namespace ClangPowerTools
{
  public class FormatOptionsInputValues
  {
    public static readonly Dictionary<string, string[]> inputValues = new Dictionary<string, string[]>()
    {
      { "AccessModifierOffset", new string[]{ "-1", "-2", "-4" }},
      { "AlignAfterOpenBracket", new string[]{ "Align", "AlwaysBreak", "DontAlign" }},
      { "AlignEscapedNewlines", new string[]{ "Left","Right", "DontAlign" }},
      { "AllowShortLambdasOnASingleLine", new string[]{ "None", "Empty", "Inline", "All" }},
      { "AllowShortFunctionsOnASingleLine", new string[]{ "None", "InlineOnly", "Empty", "Inline", "All" }},
      { "AllowShortIfStatementsOnASingleLine", new string[]{ "Never", "WithoutElse", "Always" }},
      { "AlwaysBreakAfterReturnType", new string[]{ "None", "All", "TopLevel", "AllDefinitions", "TopLevelDefinitions" }},
      { "AlwaysBreakTemplateDeclarations", new string[]{ "No", "MultiLine", "Yes" }},
      { "BreakBeforeBinaryOperators", new string[]{ "None", "NonAssignment", "All" }},
      { "BreakBeforeBraces", new string[]{ "Attach", "Linux", "Stroustrup", "Allman", "Mozilla", "WebKit", "Custom" }},
      { "BreakInheritanceList", new string[]{ "BeforeColon", "BeforeComma", "AfterColon" }},
      { "BreakConstructorInitializers", new string[]{ "BeforeColon", "BeforeComma", "AfterColon" }},
      { "ColumnLimit", new string[]{ "0", "20", "60", "80", "100", "120" }},
      { "ConstructorInitializerIndentWidth", new string[]{ "2","4", "6" }},
      { "ContinuationIndentWidth", new string[]{ "2", "4", "6" }},
      { "IncludeBlocks", new string[]{ "Preserve", "Merge", "Regroup" }},
      { "IndentPPDirectives", new string[]{ "None", "AfterHash", "BeforeHash" }},
      { "IndentWidth", new string[]{ "2", "4", "6" }},
      { "MaxEmptyLinesToKeep", new string[]{ "1", "2", "3" }},
      { "NamespaceIndentation", new string[]{ "None", "Inner", "All" }},
      { "PenaltyBreakAssignment", new string[]{ "1", "2", "3" }},
      { "PenaltyBreakBeforeFirstCallParameter", new string[]{ "1","19" }},
      { "PenaltyBreakComment", new string[]{ "300", "350" }},
      { "PenaltyBreakFirstLessLess", new string[]{ "120", "150" }},
      { "PenaltyReturnTypeOnItsOwnLine", new string[]{ "60","200", "1000" }},
      { "PointerAlignment", new string[]{ "Left","Right", "Middle" }},
      { "SpaceBeforeParens", new string[]{ "Never", "ControlStatements", "Always" }},
      { "SpacesBeforeTrailingComments", new string[]{ "1", "2", "3" }},
      { "Standard", new string[]{ "Cpp11", "Cpp14", "Cpp17", "Cpp20", "Auto" }},
      { "TabWidth", new string[]{ "4", "6", "8", "10" }},
      { "UseTab", new string[] { "Never", "ForIndentation", "ForContinuationAndIndentation", "Always" }},
    };
  }
}
