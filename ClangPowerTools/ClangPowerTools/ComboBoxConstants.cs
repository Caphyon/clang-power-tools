using System.Collections.Generic;

namespace ClangPowerTools
{
  public sealed class ComboBoxConstants
  {
    #region Constants

    public const string kCustomChecks = "custom checks";
    public const string kPredefinedChecks = "predefined checks";
    public const string kTidyFile = ".clang-tidy config file";

    public const string kDefaultHeaderFilter = ".*";
    public const string kCorrespondingHeader = "Corresponding Header";


    public const string kNone = "none";
    public const string kFile = "file";
    public const string kChromium = "Chromium";
    public const string kGoogle = "Google";
    public const string kLLVM = "LLVM";
    public const string kMozilla = "Mozilla";
    public const string kWebKit = "WebKit";

    public static readonly Dictionary<string, string> kHeaderFilterMaping = new Dictionary<string, string>
    {
      {kCorrespondingHeader, "_" },
      {"_", kCorrespondingHeader }
    };

    public const string kIncludeDirectories = "include directories";
    public const string kSystemIncludeDirectories = "system include directories";

    #endregion

  }
}
