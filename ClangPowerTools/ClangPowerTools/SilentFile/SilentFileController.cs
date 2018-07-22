using ClangPowerTools.Error;
using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using ShellConstants = Microsoft.VisualStudio.Shell.Interop.Constants;

namespace ClangPowerTools.SilentFile
{
  // Prevent visual studio to ask you if you want to reload the files
  public class SilentFileController
  {
    #region Members

    private IBuilder<SilentFileChangerModel> mSilentFileChangerBuilder;

    #endregion


    #region Constructor


    public SilentFileController()
    {
      mSilentFileChangerBuilder = new SilentFileChangerBuilder();
    }


    #endregion


    #region New guard instance

    public SilentFileChangerGuard GetSilentFileChangerGuard() => new SilentFileChangerGuard();



    #endregion


    #region Public methods


    public void SilentFiles(AsyncPackage aServiceProvider, SilentFileChangerGuard aGuard, IEnumerable<string> aFilesPath)
    {
      // silent all selected files
      aGuard.AddRange(aServiceProvider, aFilesPath);
    }

    public void SilentOpenFiles(AsyncPackage aServiceProvider, SilentFileChangerGuard aGuard, DTE2 aDte)
    {
      // silent all open documents
      foreach (Document doc in aDte.Documents)
        aGuard.Add(new SilentFileChanger(aServiceProvider, Path.Combine(doc.Path, doc.Name), true));
    }


    #endregion


    #region Private Methods


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
    public async void Suspend()
    {


      if (mIsSuspending)
        return;

      mDocData = IntPtr.Zero;
      try
      {
        var rdtService = await mSite.GetServiceAsync(typeof(SVsRunningDocumentTableService)) as IVsRunningDocumentTableService;
        var rdt = await rdtService.GetVsRunningDocumentTableAsync(mSite, new CancellationToken());

        if (rdt == null)
          return;

        ErrorHandler.ThrowOnFailure(rdt.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, mDocumentFileName,
          out IVsHierarchy hierarchy, out uint itemId, out mDocData, out uint docCookie));

        if ((docCookie == (uint)ShellConstants.VSDOCCOOKIE_NIL) || mDocData == IntPtr.Zero)
          return;

        var fileChangeService = await mSite.GetServiceAsync(typeof(SVsFileChangeService)) as IVsFileChangeService;
        var fileChange = await fileChangeService.GetVsFileChangeAsync(mSite, new CancellationToken());

        if (fileChange == null)
          return;

        mIsSuspending = true;
        ErrorHandler.ThrowOnFailure(fileChange.IgnoreFile(0, mDocumentFileName, 1));
        if (mDocData == IntPtr.Zero)
          return;

        mPersistDocData = null;
        object unknown = Marshal.GetObjectForIUnknown(mDocData);
        if (!(unknown is IVsPersistDocData))
          return;

        mPersistDocData = (IVsPersistDocData)unknown;
        if (!(mPersistDocData is IVsDocDataFileChangeControl))
          return;

        mFileChangeControl = (IVsDocDataFileChangeControl)mPersistDocData;
        if (mFileChangeControl != null)
          ErrorHandler.ThrowOnFailure(mFileChangeControl.IgnoreFileChanges(1));
      }
      catch (InvalidCastException e)
      {
        Trace.WriteLine("Exception" + e.Message);
      }
      finally
      {
        if (mDocData != IntPtr.Zero)
          Marshal.Release(mDocData);
      }
    }


    public async void Resume()
    {
      if (!mIsSuspending || mPersistDocData == null)
        return;

      if (mPersistDocData != null && mReloadDocument)
        mPersistDocData.ReloadDocData(0);

      var fileChangeService = await mSite.GetServiceAsync(typeof(SVsFileChangeService)) as IVsFileChangeService;
      var fileChange = await fileChangeService.GetVsFileChangeAsync(mSite, new CancellationToken());

      if (fileChange == null)
        return;

      mIsSuspending = false;
      ErrorHandler.ThrowOnFailure(fileChange.SyncFile(mDocumentFileName));
      ErrorHandler.ThrowOnFailure(fileChange.IgnoreFile(0, mDocumentFileName, 0));
      if (mFileChangeControl != null)
        ErrorHandler.ThrowOnFailure(mFileChangeControl.IgnoreFileChanges(0));
    }


    #endregion



  }
}
