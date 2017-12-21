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

    public static readonly Dictionary<string, string> kHeaderFilterMaping = new Dictionary<string, string>
    {
      {kCorrespondingHeader, "_" },
      {"_", kCorrespondingHeader }
    };

    #endregion

  }
}
