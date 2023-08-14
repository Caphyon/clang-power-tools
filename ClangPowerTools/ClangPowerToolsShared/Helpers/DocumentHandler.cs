using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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
      return VsServiceProvider.TryGetService(typeof(DTE2), out object dte) ? (dte as DTE2).Documents : null;
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
      return VsServiceProvider.TryGetService(typeof(DTE2), out object dte) ? (dte as DTE2).ActiveDocument : null;
    }

    /// <summary>
    /// Get the elected items
    /// </summary>
    /// <returns></returns>
    public static SelectedItems GetSelectedItems()
    {
      var vsServiceProvider = VsServiceProvider.TryGetService(typeof(DTE2), out object dte) ? (dte as DTE2) : null;

      return vsServiceProvider.SelectedItems;
    }

    /// <summary>
    /// Save all the active documents
    /// </summary>
    public static void SaveActiveDocuments()
    {
      try
      {
        GetActiveDocuments().SaveAll();
      }
      catch (System.Exception)
      {
        MessageBox.Show("Cannot get all active documents, close all tabs and try again", "Error");
      }
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

    public static void FocusActiveDocument()
    {
      var document = GetActiveDocument();
      if (document == null)
        return;

      document.Activate();
    }

    #endregion

  }
}
