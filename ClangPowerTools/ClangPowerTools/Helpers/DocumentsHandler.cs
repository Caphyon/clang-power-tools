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
