using EnvDTE;
using System.Collections.Generic;

namespace ClangPowerToolsShared.Commands.Models
{
  public class CacheProjectsItemsModel
  {
    public HashSet<Project> CacheProjects { get; set; } = new HashSet<Project>();
    public HashSet<ProjectItem> CacheProjectItems { get; set; } = new HashSet<ProjectItem>();
    public string Configuration { get; }
    public string Platform { get; }
  }
}
