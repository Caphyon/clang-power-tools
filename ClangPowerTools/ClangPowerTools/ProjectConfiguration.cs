using EnvDTE;
using EnvDTE80;

namespace ClangPowerTools
{
  public static class ProjectConfiguration
  {
    public static string GetPlatform(Project aProjectItem) => 
      aProjectItem.ConfigurationManager.ActiveConfiguration.PlatformName;

    public static string GetConfiguration(Project aProjectItem) => 
      aProjectItem.ConfigurationManager.ActiveConfiguration.ConfigurationName;
  }
}
