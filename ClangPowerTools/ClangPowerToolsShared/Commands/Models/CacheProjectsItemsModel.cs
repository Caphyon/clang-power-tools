using EnvDTE;
using System.Collections.Generic;

namespace ClangPowerToolsShared.Commands.Models
{
  /// <summary>
  /// Cache projects and Items, configuration, platform - used in script generating
  /// </summary>
  public class CacheProjectsItemsModel
  {
    public HashSet<Project> Projects { get; set; } = new HashSet<Project>();
    public HashSet<ProjectItem> ProjectItems { get; set; } = new HashSet<ProjectItem>();
    public string Configuration { get; }
    public string Platform { get; }
  }
}
