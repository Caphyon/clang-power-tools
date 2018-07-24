using ClangPowerTools.Error;
using ClangPowerTools.Services;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using ShellConstants = Microsoft.VisualStudio.Shell.Interop.Constants;

namespace ClangPowerTools.SilentFile
{
  public class SilentFileChangerBuilder : IBuilder<SilentFileChangerModel>
  {
    #region Members


    private SilentFileChangerModel mSilentFileChangerModel = new SilentFileChangerModel();
    private AsyncPackage mSite;
    private string mDocumentFileName;
    private bool mReloadDocumentFlag;

    #endregion


    #region Constructor


    /// <summary>
    /// Instance constructor
    /// </summary>
    /// <param name="aSite">Async package</param>
    /// <param name="aFileName">The file path of the file for which the changes will be ignored</param>
    /// <param name="aReloadDocument">True if the file will be reloaded. False otherwise</param>
    public SilentFileChangerBuilder(AsyncPackage aSite, string aFileName, bool aReloadDocument)
    {
      mSite = aSite;
      mDocumentFileName = aFileName;
      mReloadDocumentFlag = aReloadDocument;
    }


    #endregion


    #region IBuilder Implementation


    /// <summary>
    /// Create a new instance of silent file changer model
    /// </summary>
    public async void Build()
    {
      var docData = IntPtr.Zero;

      mSilentFileChangerModel.DocumentFileName = mDocumentFileName;
      mSilentFileChangerModel.ReloadDocumentFlag = mReloadDocumentFlag;
      mSilentFileChangerModel.IsSuspended = true;

      try
      {
        var rdtService = await mSite.GetServiceAsync(typeof(SVsRunningDocumentTableService)) as IVsRunningDocumentTableService;
        var rdt = await rdtService.GetVsRunningDocumentTableAsync(mSite, new CancellationToken());

        if (rdt == null)
          return;

        ErrorHandler.ThrowOnFailure(rdt.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, mDocumentFileName,
          out IVsHierarchy hierarchy, out uint itemId, out docData, out uint docCookie));

        if ((docCookie == (uint)ShellConstants.VSDOCCOOKIE_NIL) || docData == IntPtr.Zero)
          return;

        var fileChangeService = await mSite.GetServiceAsync(typeof(SVsFileChangeService)) as IVsFileChangeService;
        var fileChange = await fileChangeService.GetVsFileChangeAsync(mSite, new CancellationToken());

        if (fileChange == null)
          return;

        ErrorHandler.ThrowOnFailure(fileChange.IgnoreFile(0, mDocumentFileName, 1));
        if (docData == IntPtr.Zero)
          return;

        var unknown = Marshal.GetObjectForIUnknown(docData);
        if (!(unknown is IVsPersistDocData))
          return;

        mSilentFileChangerModel.PersistDocData = (IVsPersistDocData)unknown;
        if (!(mSilentFileChangerModel.PersistDocData is IVsDocDataFileChangeControl))
          return;

        mSilentFileChangerModel.FileChangeControl = (IVsDocDataFileChangeControl)mSilentFileChangerModel.PersistDocData;
        if (mSilentFileChangerModel.FileChangeControl != null)
          ErrorHandler.ThrowOnFailure(mSilentFileChangerModel.FileChangeControl.IgnoreFileChanges(1));
      }
      catch (InvalidCastException e)
      {
        Trace.WriteLine("Exception" + e.Message);
      }
      finally
      {
        if (docData != IntPtr.Zero)
          Marshal.Release(docData);
      }
    }


    /// <summary>
    /// Get the silent file changer model constructed earlier
    /// </summary>
    /// <returns>Silent file changer model</returns>
    public SilentFileChangerModel GetResult() => mSilentFileChangerModel;


    #endregion


  }
}
