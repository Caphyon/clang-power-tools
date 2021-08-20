using System;
using System.Text;

namespace ClangPowerTools
{
  public class BrowserViewModel
  {
    public Uri CreateFlagUri(string tidyCheckName)
    {
      StringBuilder sb = new();
      sb.Append(TidyConstants.FlagsUri).Append(tidyCheckName).Append(".html");
      return new Uri(sb.ToString());
    }
  }
}
