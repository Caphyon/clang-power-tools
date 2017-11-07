using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public static class ProjectConfiguration
  {
    public static string GetPlatform(DTE2 aDte)
    {
      var proj = aDte.ActiveSolutionProjects.GetValue(0) as EnvDTE.Project;
      return proj.ConfigurationManager.ActiveConfiguration.PlatformName;
    }

    public static string GetConfiguration(DTE2 aDte)
    {
      var proj = aDte.ActiveSolutionProjects.GetValue(0) as EnvDTE.Project;
      return proj.ConfigurationManager.ActiveConfiguration.ConfigurationName;
    }
  }
}
