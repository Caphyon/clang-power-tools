﻿using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public class FormatOptionsChromiumData : FormatOptionsData
  {
    public new List<IFormatOption> FormatOptions = new List<IFormatOption>()
    {
      new FormatOptionInputModel{ Name = "AccessModifierOffset", Paramater = "int", Description = "The extra indent or outdent of access modifiers, e.g. \"public:\"", Input = "-1" },
      new FormatOptionInputModel{ Name = "AlignAfterOpenBracket", Paramater = "BracketAlignmentStyle", Description = "If \"true\", horizontally aligns arguments after an open bracket.\r\nThis applies to round brackets (parentheses), angle brackets and square brackets.\r\nPossible values:\r\n- BAS_Align (in configuration: Align) Align parameters on the open bracket, e.g.:\r\n- BAS_DontAlign (in configuration: DontAlign) Don’t align, instead use ContinuationIndentWidth, e.g.:\r\n- BAS_AlwaysBreak (in configuration: AlwaysBreak) Always break after an open bracket, if the parameters don’t fit on a single line, e.g.:", Input="Align" },
      new FormatOptionToggleModel{ Name = "AlignConsecutiveMacros", Paramater = "bool", Description = "If \"true\", aligns consecutive C/C++ preprocessor macros.", BooleanCombobox = ToggleValues.False },
      new FormatOptionToggleModel{ Name = "AlignConsecutiveAssignments", Paramater = "bool", Description = "If \"true\", aligns consecutive assignments.", BooleanCombobox = ToggleValues.False },
      new FormatOptionToggleModel{ Name = "AlignConsecutiveDeclarations", Paramater = "bool", Description = "If \"true\", aligns consecutive declarations.", BooleanCombobox = ToggleValues.False},
      new FormatOptionInputModel{ Name = "AlignEscapedNewlines", Paramater = "EscapedNewlineAlignmentStyle", Description = "Options for aligning backslashes in escaped newlines.\r\nPossible values:\r\n- ENAS_DontAlign (in configuration: DontAlign) Don’t align escaped newlines\r\n- ENAS_Left (in configuration: Left) Align escaped newlines as far left as possible.\r\n- ENAS_Right (in configuration: Right) Align escaped newlines in the right-most column.", Input = "Left"},
      new FormatOptionToggleModel{ Name = "AlignOperands", Paramater = "bool", Description = "If \"true\", horizontally align operands of binary and ternary expressions.", BooleanCombobox = ToggleValues.True},
      new FormatOptionToggleModel{ Name = "AlignTrailingComments", Paramater = "bool", Description = "If \"true\", aligns trailing comments", BooleanCombobox = ToggleValues.True},
      new FormatOptionToggleModel{ Name = "AllowAllArgumentsOnNextLine", Paramater = "bool", Description = "If a function call or braced initializer list doesn’t fit on a line, allow putting all arguments onto the next line, even if BinPackArguments is false.", BooleanCombobox = ToggleValues.True},
      new FormatOptionToggleModel{ Name = "AllowAllConstructorInitializersOnNextLine", Paramater = "bool", Description = "If a constructor definition with a member initializer list doesn’t fit on a single line, allow putting all member initializers onto the next line, if `ConstructorInitializerAllOnOneLineOrOnePerLine` is true. Note that this parameter has no effect if `ConstructorInitializerAllOnOneLineOrOnePerLine` is false.", BooleanCombobox = ToggleValues.True},
      new FormatOptionToggleModel{ Name = "AllowAllParametersOfDeclarationOnNextLine", Paramater = "bool", Description = "If the function declaration doesn’t fit on a line, allow putting all parameters of a function declaration onto the next line even if BinPackParameters is false.", BooleanCombobox = ToggleValues.False},
      new FormatOptionToggleModel{ Name = "AllowShortBlocksOnASingleLine", Paramater = "bool", Description = "Dependent on the value, while (true) { continue; } can be put on a single line.", BooleanCombobox = ToggleValues.False},
      new FormatOptionToggleModel{ Name = "AllowShortCaseLabelsOnASingleLine", Paramater = "bool", Description = "If \"true\", short case labels will be contracted to a single line.", BooleanCombobox = ToggleValues.False},
      new FormatOptionInputModel{ Name = "AllowShortLambdasOnASingleLine", Paramater = "ShortLambdaStyle", Description = "Dependent on the value, auto lambda []() { return 0; } can be put on a single line.\r\nPossible values:\r\n- SLS_None (in configuration: None) Never merge lambdas into a single line.\r\n- SLS_Empty (in configuration: Empty) Only merge empty lambdas\r\n- SLS_Inline (in configuration: Inline) Merge lambda into a single line if argument of a function.\r\n- SLS_All (in configuration: All) Merge all lambdas fitting on a single line.", Input = "All"},
      new FormatOptionInputModel{ Name = "AllowShortFunctionsOnASingleLine", Paramater = "ShortFunctionStyle", Description = "Dependent on the value, int f() { return 0; } can be put on a single line.\r\nPossible values:\r\n- SFS_None (in configuration: None) Never merge functions into a single line.\r\n- SFS_InlineOnly (in configuration: InlineOnly) Only merge functions defined inside a class. Same as “inline”, except it does not implies “empty”: i.e. top level empty functions are not merged either.\r\n- SFS_Empty (in configuration: Empty) Only merge empty functions.\r\n- SFS_Inline (in configuration: Inline) Only merge functions defined inside a class. Implies “empty”.\r\n- SFS_All (in configuration: All) Merge all functions fitting on a single line.", Input = "Inline"},
      new FormatOptionInputModel{ Name = "AllowShortIfStatementsOnASingleLine", Paramater = "ShortIfStyle", Description = "If true, if (a) return; can be put on a single line.\r\nPossible values:\r\n- SIS_Never (in configuration: Never) Never put short ifs on the same line.\r\n- SIS_WithoutElse (in configuration: WithoutElse) Without else put short ifs on the same line only if the else is not a compound statement.\r\n- SIS_Always (in configuration: Always) Always put short ifs on the same line if the else is not a compound statement or not.", Input = "Never"},
      new FormatOptionToggleModel{ Name = "AllowShortLoopsOnASingleLine", Paramater = "bool", Description = "If true, while (true) continue; can be put on a single line.", BooleanCombobox = ToggleValues.False},
      new FormatOptionInputModel{ Name = "AlwaysBreakAfterReturnType", Paramater = "ReturnTypeBreakingStyle", Description = "The function declaration return type breaking style to use.\r\nPossible values:\r\n- RTBS_None (in configuration: None) Break after return type automatically. PenaltyReturnTypeOnItsOwnLine is taken into accou\r\n- RTBS_All (in configuration: All) Always break after the return type.\r\n- RTBS_TopLevel (in configuration: TopLevel) Always break after the return types of top-level functions.\r\n- RTBS_AllDefinitions (in configuration: AllDefinitions) Always break after the return type of function definitions.\r\n- RTBS_TopLevelDefinitions (in configuration: TopLevelDefinitions) Always break after the return type of top-level definitions.", Input = "None" },
      new FormatOptionToggleModel{ Name = "AlwaysBreakBeforeMultilineStrings", Paramater = "bool", Description = "If true, always break before multiline string literals.", BooleanCombobox = ToggleValues.True },
      new FormatOptionInputModel{ Name = "AlwaysBreakTemplateDeclarations", Paramater = "BreakTemplateDeclarationsStyle", Description = "The template declaration breaking style to use.\r\nPossible values:\r\n- BTDS_No (in configuration: No) Do not force break before declaration. PenaltyBreakTemplateDeclaration is taken into account\r\n- BTDS_MultiLine (in configuration: MultiLine) Force break after template declaration only when the following declaration spans multiple lines\r\n- BTDS_Yes (in configuration: Yes) Always break after template declaration.", Input = "Yes" },
      new FormatOptionToggleModel{ Name = "BinPackArguments", Paramater = "bool", Description = "If \"false\", a function declaration’s or function definition’s parameters will either all be on the same line or will have one line each.", BooleanCombobox = ToggleValues.True },
      new FormatOptionToggleModel{ Name = "BinPackParameters", Paramater = "bool", Description = "If \"false\", a function declaration’s or function definition’s parameters will either all be on the same line or will have one line each.", BooleanCombobox = ToggleValues.False },
      new FormatOptionMultipleToggleModel{ Name = "BraceWrapping", Paramater = "BraceWrappingFlags", Description = "Control of individual brace wrapping cases.\r\n If BreakBeforeBraces is set to BS_Custom, use this to specify how each individual brace case should be handled. Otherwise, this is ignored.",
                                           ToggleFlags = new List<ToggleModel>() {new ToggleModel("AfterCaseLabel", ToggleValues.False), new ToggleModel("AfterClass", ToggleValues.False), new ToggleModel("AfterControlStatement", ToggleValues.False), new ToggleModel("AfterEnum", ToggleValues.False), new ToggleModel("AfterFunction", ToggleValues.False), new ToggleModel("AfterNamespace", ToggleValues.False), new ToggleModel("AfterObjCDeclaration", ToggleValues.False), new ToggleModel("AfterStruct", ToggleValues.False), new ToggleModel("AfterUnion", ToggleValues.False),
                                           new ToggleModel("AfterExternBlock", ToggleValues.False), new ToggleModel("BeforeCatch", ToggleValues.False), new ToggleModel("BeforeElse", ToggleValues.False), new ToggleModel("IndentBraces", ToggleValues.False), new ToggleModel("SplitEmptyFunction", ToggleValues.True), new ToggleModel("SplitEmptyRecord", ToggleValues.True), new ToggleModel("SplitEmptyNamespace", ToggleValues.True)} },
      new FormatOptionInputModel{ Name = "BreakBeforeBinaryOperators", Paramater = "BinaryOperatorStyle", Description = "The way to wrap binary operators.\r\nPossible values:\r\n- BOS_None (in configuration: None) Break after operators.\r\n- BOS_NonAssignment (in configuration: NonAssignment) Break before operators that aren’t assignments.\r\n- BOS_All (in configuration: All) Break before operators.", Input = "None" },
      new FormatOptionInputModel{ Name = "BreakBeforeBraces ", Paramater = "BraceBreakingStyle", Description = "- BS_Attach (in configuration: Attach) Always attach braces to surrounding context.\r\n- BS_Linux (in configuration: Linux) Like Attach, but break before braces on function, namespace and class definitions.\r\n- BS_Stroustrup (in configuration: Stroustrup) Like Attach, but break before function definitions.\r\n- BS_Allman (in configuration: Allman) Always break before braces.", Input = "Attach" },
      new FormatOptionToggleModel{ Name = "BreakBeforeInheritanceComma", Paramater = "bool", Description = "If \"true\",  in the class inheritance expression clang-format will break before : and , if there is multiple inheritance.", BooleanCombobox = ToggleValues.False },
      new FormatOptionInputModel{ Name = "BreakInheritanceList", Paramater = "BreakInheritanceListStyle", Description = "The inheritance list style to use.\r\nPossible values:\r\n- BILS_BeforeColon (in configuration: BeforeColon) Break inheritance list before the colon and after the commas.\r\n- BILS_BeforeComma (in configuration: BeforeComma) Break inheritance list before the colon and commas, and align the commas with the colon.\r\n- BILS_AfterColon (in configuration: AfterColon) Break inheritance list after the colon and commas", Input = "BeforeColon" },
      new FormatOptionToggleModel{ Name = "BreakBeforeTernaryOperators", Paramater = "bool", Description = "If true, ternary operators will be placed after line breaks.", BooleanCombobox = ToggleValues.True },
      new FormatOptionToggleModel{ Name = "BreakConstructorInitializersBeforeComma", Paramater = "bool", Description = "Always break constructor initializers before commas and align the commas with the colon.", BooleanCombobox = ToggleValues.False },
      new FormatOptionInputModel{ Name = "BreakConstructorInitializers", Paramater = "BreakConstructorInitializersStyle", Description = "The constructor initializers style to use.\r\nPossible values:\r\n- BCIS_BeforeColon (in configuration: BeforeColon) Break constructor initializers before the colon and after the commas.\r\n- BCIS_BeforeComma (in configuration: BeforeComma) Break constructor initializers before the colon and commas, and align the commas with the colon.\r\n- BCIS_AfterColon (in configuration: AfterColon) Break constructor initializers after the colon and commas.", Input = "BeforeColon" },
      new FormatOptionToggleModel{ Name = "BreakAfterJavaFieldAnnotations", Paramater = "bool", Description = "Break after each annotation on a field in Java files", BooleanCombobox = ToggleValues.False },
      new FormatOptionToggleModel{ Name = "BreakStringLiterals", Paramater = "bool", Description = "Allow breaking string literals when formatting.", BooleanCombobox = ToggleValues.True },
      new FormatOptionInputModel{ Name = "ColumnLimit", Paramater = "unsigned", Description = "The column limit.\r\nA column limit of 0 means that there is no column limit. In this case, clang-format will respect the input’s line breaking decisions within statements unless they contradict other rules", Input = "80" },
      new FormatOptionInputModel{ Name = "CommentPragmas", Paramater = "std::string", Description = "A regular expression that describes comments with special meaning, which should not be split into lines or otherwise changed.", Input = "'^ IWYU pragma:'" },
      new FormatOptionToggleModel{ Name = "CompactNamespaces", Paramater = "bool", Description = "If \"true\", consecutive namespace declarations will be on the same line. If false, each namespace is declared on a new line.", BooleanCombobox = ToggleValues.False },
      new FormatOptionToggleModel{ Name = "ConstructorInitializerAllOnOneLineOrOnePerLine", Paramater = "bool", Description = "If the constructor initializers don’t fit on a line, put each initializer on its own line", BooleanCombobox = ToggleValues.True },
      new FormatOptionInputModel{ Name = "ConstructorInitializerIndentWidth ", Paramater = "unsigned", Description = "The number of characters to use for indentation of constructor initializer lists.", Input = "4" },
      new FormatOptionInputModel{ Name = "ContinuationIndentWidth ", Paramater = "unsigned", Description = "Indent width for line continuations", Input = "4" },
      new FormatOptionToggleModel{ Name = "Cpp11BracedListStyle", Paramater = "bool", Description = "If true, format braced lists as best suited for C++11 braced lists.\r\nImportant differences: - No spaces inside the braced list. - No line break before the closing brace. - Indentation with the continuation indent, not with the block indent\r\nFundamentally, C++11 braced lists are formatted exactly like function calls would be formatted in their place. If the braced list follows a name (e.g. a type or variable name), clang-format formats as if the {} were the parentheses of a function call with that name. If there is no name, a zero-length name is assumed.", BooleanCombobox = ToggleValues.True },
      new FormatOptionToggleModel{ Name = "DerivePointerAlignment", Paramater = "bool", Description = "If \"true\", analyze the formatted file for the most common alignment of & and *. Pointer and reference alignment styles are going to be updated according to the preferences found in the file. PointerAlignment is then used only as fallback", BooleanCombobox = ToggleValues.False },
      new FormatOptionToggleModel{ Name = "ExperimentalAutoDetectBinPacking", Paramater = "bool", Description = "If true, clang-format detects whether function calls and definitions are formatted with one parameter per line.\r\nEach call can be bin-packed, one-per-line or inconclusive. If it is inconclusive, e.g. completely on one line, but a decision needs to be made, clang-format analyzes whether there are other bin-packed cases in the input file and act accordingly.", BooleanCombobox = ToggleValues.False },
      new FormatOptionToggleModel{ Name = "FixNamespaceComments", Paramater = "bool", Description = "If \"true\", clang-format adds missing namespace end comments and fixes invalid existing ones.", BooleanCombobox = ToggleValues.True },
      new FormatOptionMultipleInputModel{ Name = "ForEachMacros", Paramater = "std::vector<std::string>", Description = "A vector of macros that should be interpreted as foreach loops instead of as function calls.", MultipleInput = "  - foreach\r\n  - Q_FOREACH\r\n  - BOOST_FOREACH" },
      new FormatOptionInputModel{ Name = "IncludeBlocks", Paramater = "IncludeBlocksStyle", Description = "Dependent on the value, multiple #include blocks can be sorted as one and divided based on category.\r\nPossible values:\r\n- IBS_Preserve (in configuration: Preserve) Sort each #include block separately.\r\n- IBS_Merge (in configuration: Merge) Merge multiple #include blocks together and sort as one.\r\n- IBS_Regroup (in configuration: Regroup) Merge multiple #include blocks together and sort as one. Then split into groups based on category priority. See IncludeCategories", Input = "Regroup" },
      new FormatOptionMultipleInputModel{ Name = "IncludeCategories", Paramater = "std::vector<std::string>", Description = "Regular expressions denoting the different #include categories used for ordering #includes.", MultipleInput = "  - Regex:           '^<ext/.*\\.h>'\r\n    Priority:        2\r\n  - Regex:           '^<.*\\.h>'\r\n    Priority:        1\r\n  - Regex:           '^<.*'\r\n    Priority:        2\r\n  - Regex:           '.*'\r\n    Priority:        3" },
      new FormatOptionInputModel{ Name = "IncludeIsMainRegex", Paramater = "std::string", Description = "Specify a regular expression of suffixes that are allowed in the file-to-main-include mapping.\r\nWhen guessing whether a #include is the “main” include (to assign category 0, see above), use this regex of allowed suffixes to the header stem. A partial match is done, so that: - “” means “arbitrary suffix” - “$” means “no suffix”\r\nFor example, if configured to “(_test)?$”, then a header a.h would be seen as the “main” include in both a.cc and a_test.cc.", Input = "'([-_](test|unittest))?$'" },
      new FormatOptionToggleModel{ Name = "IndentCaseLabels", Paramater = "bool", Description = "Indent case labels one level from the switch statement.\r\nWhen false, use the same indentation level as for the switch statement. Switch statement body is always indented one level more than case labels.", BooleanCombobox = ToggleValues.True },
      new FormatOptionInputModel{ Name = "IndentPPDirectives", Paramater = "PPDirectiveIndentStyle", Description = "The preprocessor directive indenting style to use.\r\nPossible values:\r\n- PPDIS_None (in configuration: None) Does not indent any directives\r\n- PPDIS_AfterHash (in configuration: AfterHash) Indents directives after the hash.\r\n- PPDIS_BeforeHash (in configuration: BeforeHash) Indents directives before the hash.", Input = "None" },
      new FormatOptionInputModel{ Name = "IndentWidth ", Paramater = "unsigned", Description = "The number of columns to use for indentation.", Input = "2" },
      new FormatOptionToggleModel{ Name = "IndentWrappedFunctionNames", Paramater = "bool", Description = "Indent if a function definition or declaration is wrapped after the type.", BooleanCombobox = ToggleValues.False },
      new FormatOptionInputModel{ Name = "JavaScriptQuotes", Paramater = "JavaScriptQuoteStyle", Description = "The JavaScriptQuoteStyle to use for JavaScript strings.\r\nPossible values:\r\n- JSQS_Leave (in configuration: Leave) Leave string quotes as they \r\n- JSQS_Single (in configuration: Single) Always use single quotes.\r\n- JSQS_Double (in configuration: Double) Always use double quotes.", Input = "Leave" },
      new FormatOptionToggleModel{ Name = "JavaScriptWrapImports", Paramater = "bool", Description = "Whether to wrap JavaScript import/export statements", BooleanCombobox = ToggleValues.True },
      new FormatOptionToggleModel{ Name = "KeepEmptyLinesAtTheStartOfBlocks", Paramater = "bool", Description = "If true, the empty line at the start of blocks is kept.", BooleanCombobox = ToggleValues.False },
      new FormatOptionInputModel{ Name = "MacroBlockBegin", Paramater = "std::string", Description = "A regular expression matching macros that start a block.", Input = "''" },
      new FormatOptionInputModel{ Name = "MacroBlockEnd", Paramater = "std::string", Description = "A regular expression matching macros that end a block.", Input = "''" },
      new FormatOptionInputModel{ Name = "MaxEmptyLinesToKeep ", Paramater = "unsigned", Description = "The maximum number of consecutive empty lines to keep.", Input = "1" },
      new FormatOptionInputModel{ Name = "NamespaceIndentation ", Paramater = "NamespaceIndentationKind", Description = "The indentation used for namespaces.\r\nPossible values:\r\n- NI_None (in configuration: None) Don’t indent in namespaces.\r\n- NI_Inner (in configuration: Inner) Indent only in inner namespaces (nested in other namespaces).\r\n- NI_All (in configuration: All) Indent in all namespaces.", Input = "None" },
      new FormatOptionInputModel{ Name = "ObjCBinPackProtocolList ", Paramater = "BinPackStyle", Description = "Controls bin-packing Objective-C protocol conformance list items into as few lines as possible when they go over ColumnLimit.\r\nPossible values:\r\n- BPS_Auto (in configuration: Auto) Automatically determine parameter bin-packing behavior.\r\n- BPS_Always (in configuration: Always) Always bin-pack parameters.\r\n- BPS_Never (in configuration: Never) Never bin-pack parameters.", Input = "Never" },
      new FormatOptionInputModel{ Name = "ObjCBlockIndentWidth", Paramater = "unsigned", Description = "The number of characters to use for indentation of ObjC blocks.", Input = "2" },
      new FormatOptionToggleModel{ Name = "ObjCSpaceAfterProperty", Paramater = "bool", Description = "Add a space after @property in Objective-C, i.e. use @property (readonly) instead of @property(readonly)", BooleanCombobox = ToggleValues.False },
      new FormatOptionToggleModel{ Name = "ObjCSpaceBeforeProtocolList", Paramater = "bool", Description = "Add a space in front of an Objective-C protocol list, i.e. use Foo <Protocol> instead of Foo<Protocol>.", BooleanCombobox = ToggleValues.True },
      new FormatOptionInputModel{ Name = "PenaltyBreakAssignment ", Paramater = "unsigned", Description = "The penalty for breaking around an assignment operator.", Input = "2" },
      new FormatOptionInputModel{ Name = "PenaltyBreakBeforeFirstCallParameter ", Paramater = "unsigned", Description = "The penalty for breaking a function call after \"call(\".", Input = "1" },
      new FormatOptionInputModel{ Name = "PenaltyBreakComment", Paramater = "unsigned", Description = "The penalty for each line break introduced inside a comment.", Input = "300" },
      new FormatOptionInputModel{ Name = "PenaltyBreakFirstLessLess", Paramater = "unsigned", Description = "The penalty for breaking before the first <<.", Input = "120" },
      new FormatOptionInputModel{ Name = "PenaltyBreakString", Paramater = "unsigned", Description = "The penalty for each line break introduced inside a string literal.", Input = "1000" },
      new FormatOptionInputModel{ Name = "PenaltyBreakTemplateDeclaration", Paramater = "unsigned", Description = "The penalty for breaking after template declaration.", Input = "10" },
      new FormatOptionInputModel{ Name = "PenaltyExcessCharacter", Paramater = "unsigned", Description = "The penalty for each character outside of the column limit.", Input = "1000000" },
      new FormatOptionInputModel{ Name = "PenaltyReturnTypeOnItsOwnLine", Paramater = "unsigned", Description = "Penalty for putting the return type of a function onto its own line.", Input = "200" },
      new FormatOptionInputModel{ Name = "PointerAlignment", Paramater = "PointerAlignmentStyle", Description = "Pointer and reference alignment style.\r\nPossible values:\r\n- PAS_Left (in configuration: Left) Align pointer to the left.\r\n- PAS_Right (in configuration: Right) Align pointer to the right.\r\n- PAS_Middle (in configuration: Middle) Align pointer in the middle.", Input = "Left" },
      new FormatOptionMultipleInputModel{ Name = "RawStringFormats", Paramater = "std::vector<RawStringFormat>", Description = "Defines hints for detecting supported languages code blocks in raw strings.\r\nA raw string with a matching delimiter or a matching enclosing function name will be reformatted assuming the specified language based on the style for that language defined in the .clang-format file. If no style has been defined in the .clang-format file for the specific language, a predefined style given by ‘BasedOnStyle’ is used. If ‘BasedOnStyle’ is not found, the formatting is based on llvm style. A matching delimiter takes precedence over a matching enclosing function name for determining the language of the raw string contents.", MultipleInput = " - Language: Cpp\r\n    Delimiters:\r\n      - cc\r\n      - CC\r\n      - cpp\r\n      - Cpp\r\n      - CPP\r\n      - 'c++'\r\n      - 'C++'\r\n    CanonicalDelimiter: ''\r\n    BasedOnStyle: google\r\n  - Language: TextProto\r\n    Delimiters:\r\n      - pb\r\n      - PB\r\n      - proto\r\n      - PROTO\r\n    EnclosingFunctions:\r\n      - EqualsProto\r\n      - EquivToProto\r\n      - PARSE_PARTIAL_TEXT_PROTO\r\n      - PARSE_TEST_PROTO\r\n      - PARSE_TEXT_PROTO\r\n      - ParseTextOrDie\r\n      - ParseTextProtoOrDie\r\n    CanonicalDelimiter: ''\r\n    BasedOnStyle: google" },
      new FormatOptionToggleModel{ Name = "ReflowComments", Paramater = "bool", Description = "If \"true\", clang-format will attempt to re-flow comments.", BooleanCombobox = ToggleValues.True },
      new FormatOptionToggleModel{ Name = "SortIncludes", Paramater = "bool", Description = "If \"true\", clang-format will sort #includes.", BooleanCombobox = ToggleValues.True },
      new FormatOptionToggleModel{ Name = "SortUsingDeclarations", Paramater = "bool", Description = "If \"true\", clang-format will sort using declarations.\r\nThe order of using declarations is defined as follows: Split the strings by “::” and discard any initial empty strings. The last element of each list is a non-namespace name; all others are namespace names. Sort the lists of names lexicographically, where the sort order of individual names is that all non-namespace names come before all namespace names, and within those groups, names are in case-insensitive lexicographic order.", BooleanCombobox = ToggleValues.True },
      new FormatOptionToggleModel{ Name = "SpaceAfterCStyleCast", Paramater = "bool", Description = "If \"true\", a space is inserted after C style casts.", BooleanCombobox = ToggleValues.False },
      new FormatOptionToggleModel{ Name = "SpaceAfterLogicalNot", Paramater = "bool", Description = "If \"true\", a space is inserted after the logical not operator (!).", BooleanCombobox = ToggleValues.False },
      new FormatOptionToggleModel{ Name = "SpaceAfterTemplateKeyword", Paramater = "bool", Description = "If \"true\", a space will be inserted after the ‘template’ keyword.", BooleanCombobox = ToggleValues.True },
      new FormatOptionToggleModel{ Name = "SpaceBeforeAssignmentOperators", Paramater = "bool", Description = "If \"false\", spaces will be removed before assignment operators.", BooleanCombobox = ToggleValues.True },
      new FormatOptionToggleModel{ Name = "SpaceBeforeCpp11BracedList", Paramater = "bool", Description = "If \"true\", a space will be inserted before a C++11 braced list used to initialize an object (after the preceding identifier or type)..", BooleanCombobox = ToggleValues.False },
      new FormatOptionToggleModel{ Name = "SpaceBeforeCtorInitializerColon", Paramater = "bool", Description = "If \"false\", spaces will be removed before constructor initializer colon.", BooleanCombobox = ToggleValues.True },
      new FormatOptionToggleModel{ Name = "SpaceBeforeInheritanceColon", Paramater = "bool", Description = "If \"false\", spaces will be removed before inheritance colon.", BooleanCombobox = ToggleValues.True },
      new FormatOptionInputModel{ Name = "SpaceBeforeParens", Paramater = "SpaceBeforeParensOptions", Description = "Defines in which cases to put a space before opening parentheses.\r\nPossible values:\r\n- SBPO_Never (in configuration: Never) Never put a space before opening parentheses.\r\n- SBPO_ControlStatements (in configuration: ControlStatements) Put a space before opening parentheses only after control statement keywords (for/if/while...).\r\n- SBPO_Always (in configuration: Always) Always put a space before opening parentheses, except when it’s prohibited by the syntax rules (in function-like macro definitions) or when determined by other style rules (after unary operators, opening parentheses, etc.)", Input = "ControlStatements" },
      new FormatOptionToggleModel{ Name = "SpaceBeforeRangeBasedForLoopColon", Paramater = "bool", Description = "If \"false\", spaces will be removed before range-based for loop colon.", BooleanCombobox = ToggleValues.True },
      new FormatOptionToggleModel{ Name = "SpaceInEmptyParentheses", Paramater = "bool", Description = "If \"true\", spaces may be inserted into ().", BooleanCombobox = ToggleValues.False },
      new FormatOptionInputModel{ Name = "SpacesBeforeTrailingComments", Paramater = "unsigned", Description = "The number of spaces to before trailing line comments.", Input = "2" },
      new FormatOptionToggleModel{ Name = "SpacesInAngles", Paramater = "bool", Description = "If true, spaces will be inserted after \"<\" and before \">\" in template argument lists", BooleanCombobox = ToggleValues.False },
      new FormatOptionToggleModel{ Name = "SpacesInContainerLiterals", Paramater = "bool", Description = "If true, spaces are inserted inside container literals (e.g. ObjC and JavaScript array and dict literals).", BooleanCombobox = ToggleValues.True },
      new FormatOptionToggleModel{ Name = "SpacesInCStyleCastParentheses", Paramater = "bool", Description = "If true, spaces may be inserted into C style casts.", BooleanCombobox = ToggleValues.False},
      new FormatOptionToggleModel{ Name = "SpacesInParentheses", Paramater = "bool", Description = "If \"true\", spaces will be inserted after ‘(‘ and before ‘)’.", BooleanCombobox = ToggleValues.False},
      new FormatOptionToggleModel{ Name = "SpacesInSquareBrackets ", Paramater = "bool", Description = "If \"true\", spaces will be inserted after [ and before ]. Lambdas without arguments or unspecified size array declarations will not be affected.", BooleanCombobox = ToggleValues.False},
      new FormatOptionInputModel{ Name = "Standard", Paramater = "LanguageStandard", Description = "Parse and format C++ constructs compatible with this standard.\r\nPossible values:\r\n- LS_Cpp03 (in configuration: c++03) Parse and format as C++03. Cpp03 is a deprecated alias for c++03\r\n- LS_Cpp11 (in configuration: c++11) Parse and format as C++11.\r\n- LS_Cpp14 (in configuration: c++14) Parse and format as C++14.\r\n- LS_Cpp17 (in configuration: c++17) Parse and format as C++17.\r\n- LS_Cpp20 (in configuration: c++20) Parse and format as C++20.\r\n- LS_Latest (in configuration: Latest) Parse and format using the latest supported language version. Cpp11 is a deprecated alias for Latest", Input = "Auto"},
      new FormatOptionMultipleInputModel{ Name = "StatementMacros", Paramater = "std::vector<std::string>", Description = "A vector of macros that should be interpreted as complete statements. \r\nTypical macros are expressions, and require a semi-colon to be added; sometimes this is not the case, and this allows to make clang-format aware of such cases.", MultipleInput = "  - Q_UNUSED\r\n  - QT_REQUIRE_VERSION" },
      new FormatOptionInputModel{ Name = "TabWidth", Paramater = "unsigned", Description = "The number of columns used for tab stops.", Input = "8"},
      new FormatOptionInputModel{ Name = "UseTab", Paramater = "UseTabStyle", Description = "The way to use tab characters in the resulting file.\r\nPossible values:\r\n- UT_Never (in configuration: Never) Never use tab.\r\n- UT_ForIndentation (in configuration: ForIndentation) Use tabs only for indentation.\r\n- UT_ForContinuationAndIndentation (in configuration: ForContinuationAndIndentation) Use tabs only for line continuation and indentation.\r\n- UT_Always (in configuration: Always) Use tabs whenever we need to fill whitespace that spans at least from one tab stop to the next on", Input = "Never"}
    };
  }
}