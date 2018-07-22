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


    private SilentFileChangerModel mSilentFileChangerModel;
    private AsyncPackage mSite;
    private string mDocumentFileName;


    #endregion


    #region IBuilder Implementation


    public async void Build()
    {
      var docData = IntPtr.Zero;
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

        var persistDocData = (IVsPersistDocData)unknown;
        if (!(persistDocData is IVsDocDataFileChangeControl))
          return;

        var fileChangeControl = (IVsDocDataFileChangeControl)persistDocData;

        mSilentFileChangerModel = new SilentFileChangerModel()
        {
          DocumentFileName = mDocumentFileName,
          FileChangeControl = fileChangeControl,
          PersistDocData = persistDocData,
          IsSuspended = true
        };
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

    public SilentFileChangerModel GetResult() => mSilentFileChangerModel;


    #endregion


  }
}
