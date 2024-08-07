﻿using ClangPowerTools;
using EnvDTE;
using System.Collections.Generic;
using System.Linq;

namespace ClangPowerToolsShared.Commands.Models
{
  /// <summary>
  /// Cache projects and Items, configuration, platform - used in script generating
  /// </summary>
  public class CacheProjectsItemsModel
  {
    public List<Project> Projects { get; set; } = new List<Project>();
    public List<ProjectItem> ProjectItems { get; set; } = new List<ProjectItem>();

    public List<string> ProjectsStringList
    {
      get
      {
        if (ProjectItems.Count > 0)
        {
          return ProjectItems.Select(p => p.ContainingProject.FullName).ToList().Distinct().ToList();
        }
        else if (Projects.Count > 0)
        {
          return Projects.Select(e => e.FullName).ToList();
        }
        return new();
      }
    }

    public string Configuration
    {
      get
      {
        if (ProjectItems.Count > 0)
        {
          return ProjectConfigurationHandler.GetConfiguration(ProjectItems[0].ContainingProject);
        }
        else if (Projects.Count > 0)
        {
          return ProjectConfigurationHandler.GetConfiguration(Projects[0]);
        }
        return string.Empty;
      }
    }

    public string Platform
    {
      get
      {
        if (ProjectItems.Count > 0)
        {
          return ProjectConfigurationHandler.GetPlatform(ProjectItems[0].ContainingProject);
        }
        else if (Projects.Count > 0)
        {
          return ProjectConfigurationHandler.GetPlatform(Projects[0]);
        }
        return string.Empty;
      }
    }
  }
}
