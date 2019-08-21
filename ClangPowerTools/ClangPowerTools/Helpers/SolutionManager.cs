using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.Helpers
{
  public class SolutionManager
  {

    public static bool CheckIfSolutionDoesNotContainCppProject()
    {
      DTE2 dte = (DTE2)VsServiceProvider.GetService(typeof(DTE));
      Solution solution = dte.Solution;

      if (solution == null)
      {
        return true;
      }

      foreach (var project in solution)
      {
        var proj = (Project)project;
        if (IsCppProject(proj))
        {
          return false;
        }
      }

      return true;
    }

    public static bool IsCppProject(Project project)
    {
      return project.Kind.Equals(ScriptConstants.kCppProjectGuid);
    }
  }
}
