using ClangPowerTools.Helpers;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClangPowerTools.Commands
{
  public class IgnoreCommand : BasicCommand
  {
    #region Constructor

    public IgnoreCommand(AsyncPackage aPackage, Guid aGuid, int aId) : base(aPackage, aGuid, aId)
    {
    }

    #endregion

    #region Public Methods

    public void AddIgnoreItemsToSettings<T>(List<string> documentsToIgnore, T settings, string PropertyName)
    {
      if (documentsToIgnore.Any() == false)
      {
        return;
      }

      string filesToIgnore = (string)ReflectionManager.GetProperty<T>(settings, PropertyName);

      if (filesToIgnore.Length > 0)
      {
        filesToIgnore += ";";
      }

      filesToIgnore += string.Join(";", RemoveDuplicateDocuments(documentsToIgnore, filesToIgnore));
      ReflectionManager.SetProperty(settings, PropertyName, filesToIgnore);
    }

    #endregion

    #region Private Methods

    private List<string> RemoveDuplicateDocuments(List<string> documentsToIgnore, string filesToIgnore)
    {
      List<string> trimmedDocumentToIgnore = new List<string>();

      foreach (var item in documentsToIgnore)
      {
        if (filesToIgnore.Contains(item) == false)
        {
          trimmedDocumentToIgnore.Add(item);
        }
      }
      return trimmedDocumentToIgnore;
    }

    #endregion
  }
}
