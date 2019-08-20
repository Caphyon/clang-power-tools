using ClangPowerTools.Services;
using EnvDTE;
using System.Collections.Generic;
using System.Linq;

namespace ClangPowerTools
{
  public class DocumentHandler
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
    /// Get a list of active documents
    /// </summary>
    /// <returns></returns>
    public static List<Document> GetListOfActiveDocuments()
    {
      List<Document> documents = new List<Document>();
      foreach (var item in GetActiveDocuments())
      {
        documents.Add(item as Document);
      }
      return documents;
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
    /// Save all the active documents
    /// </summary>
    public static void SaveActiveDocuments()
    {
      GetActiveDocuments().SaveAll();
    }

    /// <summary>
    /// Check if a document is open
    /// </summary>
    /// <param name="aDocument"></param>
    /// <returns>True if the document is open, false otherwise</returns>
    public static bool IsOpen(Document aSearchedDocument, List<Document> aDocuments)
    {
      Document doc = aDocuments.FirstOrDefault(currentDoc => currentDoc.FullName == aSearchedDocument.FullName);
      return doc != null;
    }

    #endregion

  }
}
