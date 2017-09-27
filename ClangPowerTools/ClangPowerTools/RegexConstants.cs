namespace ClangPowerTools
{
  public class RegexConstants
  {
    #region Constants

    public const string kFindAllPaths = @"(.\:\\[^ ]+[ \w+\\.]*[h|cpp])";
    public const string kFindLineAndColumn = @"(\d+)(?=:)";

    #endregion

  }
}
