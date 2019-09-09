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

    /// <summary>
    /// the method will add to ignored files the items meant to be ignored
    /// </summary>
    /// <typeparam name="T">template</typeparam>
    /// <param name="itemsToIgnore">list of items to ignore</param>
    /// <param name="settings">settings from SettingsProvider</param>
    /// <param name="PropertyName">name of property meant to be get and set using reflection</param>
    public void AddItemsToIgnore<T>(List<string> itemsToIgnore, T settings, string PropertyName)
    {
      if (itemsToIgnore.Any() == false)
      {
        return;
      }

      string filesToIgnore = (string)PropertyHandler.Get<T>(settings, PropertyName);

      if (filesToIgnore.Length > 0)
      {
        filesToIgnore += ";";
      }

      filesToIgnore += string.Join(";", RemoveDuplicateDocuments(itemsToIgnore, filesToIgnore));
      PropertyHandler.Set(settings, PropertyName, filesToIgnore);
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
