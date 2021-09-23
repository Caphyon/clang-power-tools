using System.Collections.Generic;

namespace ClangPowerTools
{
  public class CommandIds
  {
    #region Constants

    public static readonly HashSet<int> idsHexa = new HashSet<int>()
    {
      0x0102,
      0x1100,
      0x0101,
      0x1101,
      0x010A,
      0x1104,
      0x0109,
      0x1102,
      0x0105,
      0x1103,
      0x0104,
      0x0103,
      0x0107,
      0x0108,
      0x1032,
      0x1033,
      0X0106
    };

    public static readonly HashSet<int> idsNumeric = new HashSet<int>()
    {
      258,
      4352,
      257,
      4353,
      266,
      4356,
      265,
      4354,
      261,
      4355,
      260,
      259,
      263,
      264,
      4146,
      4147,
      262
    };

    public const int kCompileId = 0x0102;
    public const int kCompileToolbarId = 0x1100;

    public const int kTidyId = 0x0101;
    public const int kTidyToolbarId = 0x1101;

    public const int kTidyDiffId = 0x010A;
    public const int kTidyDiffToolbarId = 0x1104;

    public const int kTidyFixId = 0x0109;
    public const int kTidyFixToolbarId = 0x1102;

    public const int kClangFormat = 0x0105;
    public const int kClangFormatToolbarId = 0x1103;


    public const int kStopClang = 0x0104;
    public const int kSettingsId = 0x0103;

    public const int kIgnoreFormatId = 0x0107;
    public const int kIgnoreCompileId = 0x0108;

    public const int kITidyExportConfigId = 0x1032;
    public const int kLogoutId = 0x1033;

    public const int kJsonCompilationDatabase = 0X0106;

    #endregion
  }
}
