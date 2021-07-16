namespace ClangPowerTools.Helpers
{
  public static class JoinUtility
  {
    public static string Join(string separator, params string[] text)
    {
      return string.Join(separator, text);
    }

  }
}
