using ClangPowerTools.Helpers;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClangPowerTools.Commands
{
  public class IgnoreCommand<T> : BasicCommand
  {
    public IgnoreCommand(AsyncPackage aPackage, Guid aGuid, int aId) : base(aPackage, aGuid, aId)
    {
    }

    public void AddIgnoreItemsToSettings(List<string> documentsToIgnore, T settings, string PropertyName)
    {
      if (!documentsToIgnore.Any())
      {
        return;
      }

      string filesToIgnore = (string) ReflectionManager.GetProperty<T>(settings, PropertyName);

      if (filesToIgnore.Length > 0)
      {
        filesToIgnore += ";";
      }

      filesToIgnore += string.Join(";", RemoveDuplicateDocuments(documentsToIgnore, filesToIgnore));
      ReflectionManager.SetProperty(settings, PropertyName, filesToIgnore);
    }

    private List<string> RemoveDuplicateDocuments(List<string> documentsToIgnore, string filesToIgnore)
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


  }
}
