using ClangPowerTools.Services;
using EnvDTE;

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
      return VsServiceProvider.TryGetService(typeof(DTE), out object dte) ?
        (dte as DTE).Documents : null;
    }

    /// <summary>
    /// Get the active document
    /// </summary>
    /// <returns>Active document</returns>
    public static Document GetActiveDocument()
    {
      return VsServiceProvider.TryGetService(typeof(DTE), out object dte) ?
        (dte as DTE).ActiveDocument : null;
    }

    /// <summary>
    /// Save all the active documents
    /// </summary>
    public static void SaveActiveDocuments()
    {
      var activeDocuments = GetActiveDocuments();
      if (null != activeDocuments)
        activeDocuments.SaveAll();
    }

    /// <summary>
    /// Save all the active documents
    /// </summary>
    public static void SaveActiveDocument()
    {
      var activeDocument = GetActiveDocument();
      if (null != activeDocument)
        activeDocument.Save();
    }

    #endregion

  }
}
