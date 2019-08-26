using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ClangPowerTools
{
  [Serializable]
  [JsonConverter(typeof(StringEnumConverter))]
  public enum ClangFormatStyle
  {
    [EnumMember(Value = "file")]
    file = 0,
    [EnumMember(Value = "Chromium")]
    Chromium,
    [EnumMember(Value = "Google")]
    Google,
    [EnumMember(Value = "LLVM")]
    LLVM,
    [EnumMember(Value = "Mozilla")]
    Mozilla,
    [EnumMember(Value = "WebKit")]
    WebKit
  }

  [Serializable]
  [JsonConverter(typeof(StringEnumConverter))]
  public enum ClangFormatFallbackStyle
  {
    [EnumMember(Value = "none")]
    none = 0,
    [EnumMember(Value = "file")]
    file,
    [EnumMember(Value = "Chromium")]
    Chromium,
    [EnumMember(Value = "Google")]
    Google,
    [EnumMember(Value = "LLVM")]
    LLVM,
    [EnumMember(Value = "Mozilla")]
    Mozilla,
    [EnumMember(Value = "WebKit")]
    WebKit
  }

  [Serializable]
  [JsonConverter(typeof(StringEnumConverter))]
  public enum ClangGeneralAdditionalIncludes
  {
    [XmlEnum(Name = "include directories")]
    [EnumMember(Value = "IncludeDirectories")]
    IncludeDirectories,

    [XmlEnum(Name = "system include directories")]
    [EnumMember(Value = "SystemIncludeDirectories")]
    SystemIncludeDirectories
  }


  [Serializable]
  public enum ClangTidyHeaderFilters
  {
    [XmlEnum(Name = ".*")]
    DefaultHeaderFilter,

    [XmlEnum(Name = "_")]
    CorrespondingHeader
  }


  [Serializable]
  [JsonConverter(typeof(StringEnumConverter))]
  public enum ClangTidyUseChecksFrom
  {
    [XmlEnum(Name = "predefined checks")]
    [EnumMember(Value = "PredefinedChecks")]
    PredefinedChecks,

    [XmlEnum(Name = "custom checks")]
    [EnumMember(Value = "CustomChecks")]
    CustomChecks,

    [XmlEnum(Name = ".clang-tidy config file")]
    [EnumMember(Value = "TidyFile")]
    TidyFile
  }

  [Serializable]
  [JsonConverter(typeof(StringEnumConverter))]
  public enum ClangTidyChecksFrom
  {
    [XmlEnum(Name = "checks")]
    [EnumMember(Value = "Checks")]
    Checks,

    [XmlEnum(Name = ".clang-tidy config file")]
    [EnumMember(Value = "TidyFile")]
    TidyFile
  }

  public sealed class ComboBoxConstants
  {
    #region Constants

    public const string kCustomChecks = "custom checks";
    public const string kPredefinedChecks = "predefined checks";
    public const string kTidyFile = ".clang-tidy config file";

    public const string kDefaultHeaderFilter = ".*";
    public const string kCorrespondingHeaderValue = "_";
    public const string kCorrespondingHeaderName = "Corresponding Header";

    public const string kNone = "none";
    public const string kFile = "file";
    public const string kChromium = "Chromium";
    public const string kGoogle = "Google";
    public const string kLLVM = "LLVM";
    public const string kMozilla = "Mozilla";
    public const string kWebKit = "WebKit";

    public const string kIncludeDirectories = "include directories";
    public const string kSystemIncludeDirectories = "system include directories";

    #endregion

  }
}
