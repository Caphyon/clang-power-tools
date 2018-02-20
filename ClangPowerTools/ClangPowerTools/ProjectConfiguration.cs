using EnvDTE;
using System;

namespace ClangPowerTools
{
  public static class ProjectConfiguration
  {
    public static string GetPlatform(Project aProjectItem)
    {
      try
      {
        return aProjectItem.ConfigurationManager.ActiveConfiguration.PlatformName;
      }
      catch (Exception)
      {
      }
      return string.Empty;
    }
    public static string GetConfiguration(Project aProjectItem)
    {
      try
      {
        return aProjectItem.ConfigurationManager.ActiveConfiguration.ConfigurationName;
      }
      catch (Exception)
      {
      }
      return string.Empty;
    }
  }
}
