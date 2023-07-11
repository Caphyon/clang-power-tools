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
    [EnumMember(Value = "Microsoft")]
    Microsoft,
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
    [EnumMember(Value = "Chromium")]
    Chromium,
    [EnumMember(Value = "Google")]
    Google,
    [EnumMember(Value = "LLVM")]
    LLVM,
    [EnumMember(Value = "Microsoft")]
    Microsoft,
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
  [JsonConverter(typeof(StringEnumConverter))]
  public enum ClangVerbosityLevel
  {
    [XmlEnum(Name = "Error")]
    [EnumMember(Value = "Error")]
    Error,

    [XmlEnum(Name = "Warning")]
    [EnumMember(Value = "Warning")]
    Warning,

    [XmlEnum(Name = "Info")]
    [EnumMember(Value = "Info")]
    Info,

    [XmlEnum(Name = "Verbose")]
    [EnumMember(Value = "Verbose")]
    Verbose,

    [XmlEnum(Name = "Debug")]
    [EnumMember(Value = "Debug")]
    Debug,
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
  public enum ToggleValues
  {
    [XmlEnum(Name = "true")]
    [EnumMember(Value = "true")]
    True,
    [XmlEnum(Name = "false")]
    [EnumMember(Value = "false")]
    False
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
