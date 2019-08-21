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

    public static bool ContainsCppProject()
    {
      DTE2 dte = (DTE2)VsServiceProvider.GetService(typeof(DTE));
      Solution solution = dte.Solution;

      if (solution == null)
      {
        return false;
      }

      return AnyCppProject(solution);
    }

    public static bool AnyCppProject(Solution solution)
    {
      foreach (var project in solution)
      {
        if (IsCppProject((Project)project))
        {
          return true;
        }
      }
      return false;
    }

    public static bool IsCppProject(Project project)
    {
      return project.Kind.Equals(ScriptConstants.kCppProjectGuid);
    }
  }
}
