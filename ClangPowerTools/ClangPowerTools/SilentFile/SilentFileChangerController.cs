using ClangPowerTools.Error;
using ClangPowerTools.Services;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ClangPowerTools.SilentFile
{
  /// <summary>
  /// Prevent visual studio to ask you if you want to reload the files
  /// </summary>
  public class SilentFileChangerController : IDisposable
  {
    #region Members


    /// <summary>
    /// Collection of all silent file changer models necessary when tidy-fix is executed
    /// </summary>
    private HashSet<SilentFileChangerModel> mFileChangers =
          new HashSet<SilentFileChangerModel>(new SilentFileChangerEqualityComparer());

    /// <summary>
    /// The async package instance
    /// </summary>
    private AsyncPackage mAsyncPackage;

    #endregion


    #region Constructor 

    /// <summary>
    /// Instance Constructor
    /// </summary>
    /// <param name="aAsyncPackage"></param>
    public SilentFileChangerController(AsyncPackage aAsyncPackage) => mAsyncPackage = aAsyncPackage;


    #endregion


    #region Methods




    #region Public methods


    /// <summary>
    /// Silent all files from a IEnumerable data collection
    /// </summary>
    /// <param name="aServiceProvider">Async package instance</param>
    /// <param name="aFilesPath">Files path collection for the files for which the changes will be ignored</param>
    public void SilentFiles(IEnumerable<string> aFilesPath)
    {
      foreach (var filePath in aFilesPath)
      {
        var silentFileChanger = GetNewSilentFileChanger(filePath);
        if (null != silentFileChanger.FileChangeControl)
          ErrorHandler.ThrowOnFailure(silentFileChanger.FileChangeControl.IgnoreFileChanges(1));
      }
    }


    /// <summary>
    /// Silent all open files extracted from a DTE documents data collection
    /// </summary>
    /// <param name="aServiceProvider">Async package instance</param>
    /// <param name="aDte">DTE instance to collect all the documents </param>
    public void SilentFiles(Documents aDocuments)
    {
      foreach (Document doc in aDocuments)
      {
        var silentFileChanger = GetNewSilentFileChanger(Path.Combine(doc.Path, doc.Name));
        if (null != silentFileChanger.FileChangeControl)
          ErrorHandler.ThrowOnFailure(silentFileChanger.FileChangeControl.IgnoreFileChanges(1));
      }
    }


    #region IDisposable Implementation

    /// <summary>
    /// Abort the ignore changes action for all stored files
    /// </summary>
    public void Dispose()
    {
      foreach (var silentFileChanger in mFileChangers)
        Resume(silentFileChanger);
    }


    #endregion


    #endregion


    #region Private Methods


    /// <summary>
    /// Create a new silent file changer model object
    /// </summary>
    /// <param name="aServiceProvider">Async package instance</param>
    /// <param name="aFilePath">The file path of the file for which the changes will be ignored</param>
    private SilentFileChangerModel GetNewSilentFileChanger(string aFilePath)
    {
      IBuilder<SilentFileChangerModel> silentFileChangerBuilder = new SilentFileChangerBuilder(mAsyncPackage, aFilePath, true);
      silentFileChangerBuilder.Build();
      return silentFileChangerBuilder.GetResult();
    }


    /// <summary>
    /// Stop to ignore the changes of a specific file
    /// </summary>
    public async void Resume(SilentFileChangerModel aSilentFileChanger)
    {
      if (!aSilentFileChanger.IsSuspended || null == aSilentFileChanger.PersistDocData)
        return;

      if (null != aSilentFileChanger.PersistDocData && aSilentFileChanger.ReloadDocumentFlag)
        aSilentFileChanger.PersistDocData.ReloadDocData(0);

      var fileChangeService = await mAsyncPackage.GetServiceAsync(typeof(SVsFileChangeService)) as IVsFileChangeService;
      var fileChange = await fileChangeService.GetVsFileChangeAsync(mAsyncPackage, new CancellationToken());

      if (fileChange == null)
        return;

      aSilentFileChanger.IsSuspended = false;
      ErrorHandler.ThrowOnFailure(fileChange.SyncFile(aSilentFileChanger.DocumentFileName));
      ErrorHandler.ThrowOnFailure(fileChange.IgnoreFile(0, aSilentFileChanger.DocumentFileName, 0));
      if (aSilentFileChanger.FileChangeControl != null)
        ErrorHandler.ThrowOnFailure(aSilentFileChanger.FileChangeControl.IgnoreFileChanges(0));
    }


    #endregion


    #endregion

  }
}
