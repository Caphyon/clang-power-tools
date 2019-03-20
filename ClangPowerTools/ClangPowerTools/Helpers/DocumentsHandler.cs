using ClangPowerTools.Services;
using EnvDTE;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public class DocumentsHandler
  {
    #region Public Methods

    /// <summary>
    /// Get active documents
    /// </summary>
    /// <returns>Active documents</returns>
    public static Documents GetActiveDocuments()
    {
      return VsServiceProvider.TryGetService(typeof(DTE), out object dte) ? (dte as DTE).Documents : null;
    }

    /// <summary>
    /// Get the active document
    /// </summary>
    /// <returns>Active document</returns>
    public static Document GetActiveDocument()
    {
      return VsServiceProvider.TryGetService(typeof(DTE), out object dte) ? (dte as DTE).ActiveDocument : null;
    }

    /// <summary>
    /// Get the elected items
    /// </summary>
    /// <returns></returns>
    public static SelectedItems GetSelectedItems()
    {
      DTE vsServiceProvider = VsServiceProvider.TryGetService(typeof(DTE), out object dte) ? (dte as DTE) : null;

      return vsServiceProvider.SelectedItems;
    }

    /// <summary>
    /// Get the name of the active document
    /// </summary>
    public static List<string> GetDocumentsToIgnore()
    {
      List<string> documentsToIgnore = new List<string>();
      DTE vsServiceProvider = VsServiceProvider.TryGetService(typeof(DTE), out object dte) ? (dte as DTE) : null;

      Document activeDocument = vsServiceProvider.ActiveDocument;
      SelectedItems selectedDocuments = vsServiceProvider.SelectedItems;

      if (selectedDocuments.Count == 1 && selectedDocuments.Item(1).Name == activeDocument.Name)
      {
        documentsToIgnore.Add(activeDocument.Name);
        return documentsToIgnore;
      }

      if (selectedDocuments.Count > 0)
      {
        for (int i = 1; i <= selectedDocuments.Count; i++)
        {
          documentsToIgnore.Add(selectedDocuments.Item(i).Name);
        }
      }
      return documentsToIgnore;
    }

    /// <summary>
    /// Save all the active documents
    /// </summary>
    public static void SaveActiveDocuments()
    {
      var activeDocuments = GetActiveDocuments();
      if (null != activeDocuments && 0 < activeDocuments.Count)
        activeDocuments.SaveAll();
    }

    #endregion

  }
}
