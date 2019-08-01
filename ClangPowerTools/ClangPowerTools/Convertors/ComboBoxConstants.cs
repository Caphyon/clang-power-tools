using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ClangPowerTools
{
  [Serializable]
  public enum ClangFormatStyle
  {
    [XmlEnum(Name = "file")]
    file = 0,
    [XmlEnum(Name = "Chromium")]
    Chromium,
    [XmlEnum(Name = "Google")]
    Google,
    [XmlEnum(Name = "LLVM")]
    LLVM,
    [XmlEnum(Name = "Mozilla")]
    Mozilla,
    [XmlEnum(Name = "WebKit")]
    WebKit
  }

  [Serializable]
  public enum ClangFormatFallbackStyle
  {
    [XmlEnum(Name = "none")]
    none = 0,
    [XmlEnum(Name = "file")]
    file,
    [XmlEnum(Name = "Chromium")]
    Chromium,
    [XmlEnum(Name = "Google")]
    Google,
    [XmlEnum(Name = "LLVM")]
    LLVM,
    [XmlEnum(Name = "Mozilla")]
    Mozilla,
    [XmlEnum(Name = "WebKit")]
    WebKit
  }

  [Serializable]
  [JsonConverter(typeof(StringEnumConverter))]
  public enum ClangGeneralAdditionalIncludes
  {
    [XmlEnum(Name = "include directories")]
    [EnumMember(Value = "include directories")]
    IncludeDirectories,

    [XmlEnum(Name = "system include directories")]
    [EnumMember(Value = "system include directories")]
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
  public enum ClangTidyUseChecksFrom
  {
    [XmlEnum(Name = "predefined checks")]
    PredefinedChecks,

    [XmlEnum(Name = "custom checks")]
    CustomChecks,

    [XmlEnum(Name = ".clang-tidy config file")]
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
