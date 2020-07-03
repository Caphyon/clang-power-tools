using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace ClangPowerTools.MVVM.LicenseValidation
{
  [Serializable]
  [JsonConverter(typeof(StringEnumConverter))]
  public enum LicenseType
  {
    [EnumMember(Value = "Commercial")]
    Commercial,

    [EnumMember(Value = "Personal")]
    Personal,

    [EnumMember(Value = "Trial")]
    Trial,

    [EnumMember(Value = "SessionExpired")]
    SessionExpired,

    [EnumMember(Value = "NoLicense")]
    NoLicense
  }
}
