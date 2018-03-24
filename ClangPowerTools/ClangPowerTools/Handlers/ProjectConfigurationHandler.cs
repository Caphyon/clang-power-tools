using EnvDTE;

namespace ClangPowerTools
{
  public class ProjectConfigurationHandler
  {
    #region Public Methods

    public static string GetPlatform(Project aProject)
    {
      var succes = GetActiveConfiguration(aProject, out Configuration configuration);
      if (false == succes)
        return string.Empty;

      return configuration.PlatformName;
    }

    public static string GetConfiguration(Project aProject)
    {
      var succes = GetActiveConfiguration(aProject, out Configuration configuration);
      if (false == succes)
        return string.Empty;

      return configuration.ConfigurationName;
    }

    #endregion

    #region Private Methods

    private static bool GetActiveConfiguration(Project aProject, out Configuration aConfiguration)
    {
      aConfiguration = null;

      var configurationManager = aProject.ConfigurationManager;
      if (null == configurationManager)
        return false;

      aConfiguration = configurationManager.ActiveConfiguration;
      if (null == aConfiguration)
        return false;

      return true;
    }

    #endregion



  }
}
