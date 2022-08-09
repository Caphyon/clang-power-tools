using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ClangPowerTools
{
  public class RunningDocTableEvents : IVsRunningDocTableEvents3
  {
    #region Members

    private RunningDocumentTable mRunningDocumentTable;

    public delegate void OnBeforeSaveHandler(object sender, Document document);
    public delegate void OnBeforeActiveDocumentChange(object sender, Document document);

    public event OnBeforeSaveHandler BeforeSave;
    public event OnBeforeActiveDocumentChange BeforeActiveDocumentChange;

    #endregion

    #region Constructor

    public RunningDocTableEvents(Package aPackage)
    {
      try
      {
        mRunningDocumentTable = new RunningDocumentTable(aPackage);
        mRunningDocumentTable.Advise(this);
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    #endregion

    #region IVsRunningDocTableEvents3 implementation

    public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
    {
      return VSConstants.S_OK;
    }

    public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
    {
      return VSConstants.S_OK;
    }

    public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
    {
      return VSConstants.S_OK;
    }

    public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
    {
      return VSConstants.S_OK;
    }

    public int OnAfterSave(uint docCookie)
    {
      return VSConstants.S_OK;
    }

    public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
    {
      if (fFirstShow == 0)
        return VSConstants.S_OK;

      if (null == BeforeActiveDocumentChange)
        return VSConstants.S_OK;

      var document = FindDocumentByCookie(docCookie);
      if (null == document)
        return VSConstants.S_OK;

      OnBeforeActiveDocumentChange handler = BeforeActiveDocumentChange;
      if (handler != null)
        handler(this, document);

      return VSConstants.S_OK;
    }

    public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
    {
      return VSConstants.S_OK;
    }

    public int OnBeforeSave(uint docCookie)
    {
      try
      {
        if (null == BeforeSave)
          return VSConstants.S_OK;

        var document = FindDocumentByCookie(docCookie);
        if (null == document)
          return VSConstants.S_OK;

        bool acceptedExtension = ScriptConstants.kExtendedAcceptedFileExtensions.Contains(Path.GetExtension(document.Name));
        if (acceptedExtension == false)
          return VSConstants.S_OK;

        OnBeforeSaveHandler handler = BeforeSave;
        if (handler != null)
          handler(this, document);
      }
      catch (Exception e)
      {
        MessageBox.Show("Error while running clang command on save. " + e.Message, "Clang Power Tools", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }

      return VSConstants.S_OK;
    }

    #endregion

    #region Private Methods

    private Document FindDocumentByCookie(uint docCookie)
    {
      Document document = null;
      try
      {
        var documentInfo = mRunningDocumentTable.GetDocumentInfo(docCookie);

        if (VsServiceProvider.TryGetService(typeof(DTE2), out object dte))
          document = (dte as DTE2).Documents.Cast<Document>().FirstOrDefault(doc => doc.FullName == documentInfo.Moniker);
      }
      catch (Exception)
      {
        //TODO find solution for DTE not intialized correctly sometimes
        // MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }

      return document;
    }

    #endregion

  }
}
