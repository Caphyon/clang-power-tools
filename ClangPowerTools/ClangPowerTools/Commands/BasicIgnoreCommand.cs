using ClangPowerTools.Helpers;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClangPowerTools.Commands
{
  public class BasicIgnoreCommand<T> : BasicCommand
  {
    public BasicIgnoreCommand(AsyncPackage aPackage, Guid aGuid, int aId) : base(aPackage, aGuid, aId)
    {
    }

    public void AddIgnoreFilesToSettings(List<string> documentsToIgnore, T settings)
    {
      if (!documentsToIgnore.Any())
      {
        return;
      }

      string filesToIgnore = (string) ReflectionManager.GetProperty<T>(settings, "FilesToIgnore");

      if (filesToIgnore.Length > 0)
      {
        filesToIgnore += ";";
      }

      filesToIgnore += string.Join(";", RemoveDuplicateFiles(documentsToIgnore, filesToIgnore));
      ReflectionManager.SetProperty(settings, "FilesToIgnore", filesToIgnore);
    }

    private List<string> RemoveDuplicateFiles(List<string> documentsToIgnore, string filesToIgnore)
    {
      List<string> trimmedDocumentToIgnore = new List<string>();

      foreach (var item in documentsToIgnore)
      {
        if (!filesToIgnore.Contains(item))
        {
          trimmedDocumentToIgnore.Add(item);
        }
      }
      return trimmedDocumentToIgnore;
    }

    public void AddIgnoreProjectsToSettings(List<string> documentsToIgnore)
    {
      if (!documentsToIgnore.Any())
      {
        return;
      }
      var settings = SettingsProvider.GeneralSettings;

      if (settings.ProjectsToIgnore.Length > 0)
      {
        settings.ProjectsToIgnore += ";";
      }
      settings.ProjectsToIgnore += string.Join(";", RemoveDuplicateProjects(documentsToIgnore, settings));
      settings.SaveSettingsToStorage();
    }

    private List<string> RemoveDuplicateProjects(List<string> documentsToIgnore, ClangGeneralOptionsView settings)
    {
      List<string> trimmedDocumentToIgnore = new List<string>();

      foreach (var item in documentsToIgnore)
      {
        if (!settings.ProjectsToIgnore.Contains(item))
        {
          trimmedDocumentToIgnore.Add(item);
        }
      }
      return trimmedDocumentToIgnore;
    }
  }
}
