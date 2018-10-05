using ClangPowerTools.Builder;
using ClangPowerTools.Services;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ShellConstants = Microsoft.VisualStudio.Shell.Interop.Constants;

namespace ClangPowerTools.SilentFile
{
  public class SilentFileChangerBuilder : IBuilder<SilentFileChangerModel>
  {
    #region Members

    private SilentFileChangerModel mSilentFileChangerModel;

    #endregion


    #region Constructor


    /// <summary>
    /// Instance constructor
    /// </summary>
    /// <param name="aSite">Async package</param>
    /// <param name="aFileName">The file path of the file for which the changes will be ignored</param>
    /// <param name="aReloadDocument">True if the file will be reloaded. False otherwise</param>
    public SilentFileChangerBuilder(string aFileName, bool aReloadDocument)
    {
      mSilentFileChangerModel = new SilentFileChangerModel()
      {
        DocumentFileName = aFileName,
        ReloadDocumentFlag = aReloadDocument,
        IsSuspended = true
      };
    }


    #endregion


    #region IBuilder Implementation


    /// <summary>
    /// Create a new instance of silent file changer model
    /// </summary>
    public void Build()
    {
      var docData = IntPtr.Zero;
      try
      {
        if (!VsServiceProvider.TryGetService(typeof(SVsRunningDocumentTable), out object vsRunningDocTable) || null == vsRunningDocTable as IVsRunningDocumentTable)
          return;

        ErrorHandler.ThrowOnFailure((vsRunningDocTable as IVsRunningDocumentTable).FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, 
          mSilentFileChangerModel.DocumentFileName, out IVsHierarchy hierarchy, out uint itemId, out docData, out uint docCookie));

        if ((docCookie == (uint)ShellConstants.VSDOCCOOKIE_NIL) || docData == IntPtr.Zero)
          return;

        if (!VsServiceProvider.TryGetService(typeof(SVsFileChangeEx), out object vsFileChange) || null == vsFileChange as IVsFileChangeEx)
          return;

        ErrorHandler.ThrowOnFailure((vsFileChange as IVsFileChangeEx).IgnoreFile(0, mSilentFileChangerModel.DocumentFileName, 1));
        if (docData == IntPtr.Zero)
          return;

        var unknown = Marshal.GetObjectForIUnknown(docData);
        if (!(unknown is IVsPersistDocData))
          return;

        mSilentFileChangerModel.PersistDocData = (IVsPersistDocData)unknown;
        if (!(mSilentFileChangerModel.PersistDocData is IVsDocDataFileChangeControl))
          return;

        mSilentFileChangerModel.FileChangeControl = mSilentFileChangerModel.PersistDocData as IVsDocDataFileChangeControl;
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
