using ClangPowerTools.Commands;
using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Views;
using ClangPowerTools.Output;
using ClangPowerTools.Services;
using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.Helpers;
using ClangPowerToolsShared.MVVM;
using ClangPowerToolsShared.MVVM.Constants;
using ClangPowerToolsShared.MVVM.Provider;
using ClangPowerToolsShared.MVVM.Views.ToolWindows;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools
{
  public class ClangPowerToolsPackageImpl : IVsSolutionEvents, IVsSolutionLoadEvents, IVsSolutionEvents7
  {
    #region Members

    /// <summary>
    /// RunPowerShellCommandPackage GUID string.
    /// </summary>
    public const string PackageGuidString = "f564f9d3-01ae-493e-883b-18deebdb975e";

    private uint mHSolutionEvents = uint.MaxValue;
    private ErrorWindowController mErrorWindowController;
    private CommandController mCommandController;

    private CommandEvents mCommandEvents;
    private BuildEvents mBuildEvents;
    private DTEEvents mDteEvents;
    private WindowEvents windowEvents;

    private AsyncPackage mPackage;
    #endregion


    #region Constructor


    /// <summary>
    /// Initializes a new instance of the <see cref="TidyCommand"/> class.
    /// </summary>
    public ClangPowerToolsPackageImpl(AsyncPackage aPackage)
    {
      mPackage = aPackage;
    }

    #endregion


    #region Initialize Package


    /// <summary>
    /// Initialization of the package; this method is called right after the package is sited, so this is the place
    /// where you can put all the initialization code that rely on services provided by VisualStudio.
    /// </summary>
    public async Task InitializeAsync()
    {
      try
      {
        await RegisterVsServicesAsync();

        mCommandController = new CommandController(mPackage);
        CommandControllerInstance.CommandController = mCommandController;

        var vsOutputWindow = VsServiceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;

        PowerShellWrapper.mOutputWindowController = new OutputWindowController();
        PowerShellWrapper.mOutputWindowController.Initialize(mPackage, vsOutputWindow);

        CommandControllerInstance.CommandController.mRunningDocTableEvents = new RunningDocTableEvents(mPackage);
        mErrorWindowController = new ErrorWindowController(mPackage);

        #region Get Pointer to IVsSolutionEvents

        if (VsServiceProvider.TryGetService(typeof(SVsSolution), out object vsSolutionService))
        {
          var vsSolution = vsSolutionService as IVsSolution;
          UnadviseSolutionEvents(vsSolution);
          AdviseSolutionEvents(vsSolution);
        }

        #endregion

        // Get-Set the build and command events from DTE
        if (VsServiceProvider.TryGetService(typeof(DTE2), out object dte))
        {
          var dte2 = dte as DTE2;

          mBuildEvents = dte2.Events.BuildEvents;
          mCommandEvents = dte2.Events.CommandEvents;
          mDteEvents = dte2.Events.DTEEvents;
          windowEvents = dte2.Events.WindowEvents;
        }

        var findToolWindowHandler = new FindToolWindowHandler();
        findToolWindowHandler.Initialize();

        var settingsHandler = new SettingsHandler();
        settingsHandler.InitializeSettings();
        await settingsHandler.InitializeAccountSettingsAsync();

        string version = SettingsProvider.GeneralSettingsModel.Version;

        ShowToolbar(version);
        UpdateVersion(version);

        await mCommandController.InitializeCommandsAsync(mPackage);

        await RegisterToEventsAsync();
      }
      catch (Exception ex)
      {
      }
    }

    #endregion


    #region Get Pointer to IVsSolutionEvents


    private void AdviseSolutionEvents(IVsSolution aVsSolution)
    {
      try
      {
        aVsSolution?.AdviseSolutionEvents(this, out mHSolutionEvents);
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void UnadviseSolutionEvents(IVsSolution aVsSolution)
    {
      if (null == aVsSolution)
        return;

      if (uint.MaxValue != mHSolutionEvents)
      {
        aVsSolution.UnadviseSolutionEvents(mHSolutionEvents);
        mHSolutionEvents = uint.MaxValue;
      }

      aVsSolution = null;
    }

    #endregion


    #region IVsSolutionEvents Implementation


    public int OnAfterOpenProject(IVsHierarchy aPHierarchy, int aFAdded)
    {
      CreateCacheRepository();
      return VSConstants.S_OK;
    }

    public int OnQueryCloseProject(IVsHierarchy aPHierarchy, int aFRemoving, ref int aPfCancel)
    {
      DeleteTempSolution();
      HideToolWindow();
      DeleteCacheReporitory();
      DeleteFromFindToolWindowHistory();
      return VSConstants.S_OK;
    }

    public int OnBeforeCloseProject(IVsHierarchy aPHierarchy, int aFRemoved)
    {
      aPHierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out object projectObject);
      if (projectObject is Project project)
        mErrorWindowController.RemoveErrors(aPHierarchy);
      return VSConstants.S_OK;
    }

    public int OnAfterLoadProject(IVsHierarchy aPStubHierarchy, IVsHierarchy aPRealHierarchy)
    {
      DeleteTempSolution();
      HideToolWindow();
      return VSConstants.S_OK;
    }

    public int OnQueryUnloadProject(IVsHierarchy aPRealHierarchy, ref int aPfCancel)
    {
      DeleteTempSolution();
      HideToolWindow();
      return VSConstants.S_OK;
    }

    public int OnBeforeUnloadProject(IVsHierarchy aPRealHierarchy, IVsHierarchy aPStubHierarchy)
    {
      DeleteTempSolution();
      HideToolWindow();
      return VSConstants.S_OK;
    }


    public int OnAfterOpenSolution(object aPUnkReserved, int aFNewSolution)
    {
      CreateCacheRepository();
      return VSConstants.S_OK;
    }


    public int OnQueryCloseSolution(object aPUnkReserved, ref int aPfCancel)
    {
      return VSConstants.S_OK;
    }

    public int OnBeforeCloseSolution(object aPUnkReserved)
    {
      DeleteTempSolution();
      HideToolWindow();
      return VSConstants.S_OK;
    }

    public int OnAfterCloseSolution(object aPUnkReserved)
    {
      return VSConstants.S_OK;
    }

    #endregion


    #region IVsSolutionLoadEvents implementation


    public int OnBeforeOpenSolution(string pszSolutionFilename)
    {
      CreateCacheRepository();
      return VSConstants.S_OK;
    }

    public int OnBeforeBackgroundSolutionLoadBegins()
    {
      return VSConstants.S_OK;
    }

    public int OnQueryBackgroundLoadProjectBatch(out bool pfShouldDelayLoadToNextIdle)
    {
      pfShouldDelayLoadToNextIdle = false;
      return VSConstants.S_OK;
    }

    public int OnBeforeLoadProjectBatch(bool fIsBackgroundIdleBatch)
    {
      return VSConstants.S_OK;
    }

    public int OnAfterLoadProjectBatch(bool fIsBackgroundIdleBatch)
    {
      return VSConstants.S_OK;
    }

    public int OnAfterBackgroundSolutionLoadComplete()
    {
      return VSConstants.S_OK;
    }

    #endregion


    #region IVsSolution7 implementation

    public void OnAfterOpenFolder(string folderPath)
    {
    }

    public void OnBeforeCloseFolder(string folderPath)
    {
    }

    public void OnQueryCloseFolder(string folderPath, ref int pfCancel)
    {
    }

    public void OnAfterCloseFolder(string folderPath)
    {
    }

    public void OnAfterLoadAllDeferredProjects()
    {
    }

    #endregion

    #region Private Methods

    private void UpdateVersion(string version)
    {
      var generalSettingsModel = SettingsProvider.GeneralSettingsModel;

      string currentVersion = PackageUtility.GetVersion();
      if (string.IsNullOrWhiteSpace(currentVersion) == false && 0 > string.Compare(version, currentVersion))
      {
        generalSettingsModel.Version = currentVersion;

        var findToolWindowHandler = new FindToolWindowHandler();
        findToolWindowHandler.SaveMatchersHistoryData();

        var settingsHandler = new SettingsHandler();
        settingsHandler.SaveSettings();

        //var freeTrialController = new FreeTrialController();
        //bool activeLicense = await new LocalLicenseValidator().ValidateAsync();

        //if (activeLicense)
        //  freeTrialController.MarkAsExpired();

        ReleaseNotesView.WasShown = false;
      }
    }

    private void ShowToolbar(string version)
    {
      // Detect the first install 
      if (!string.IsNullOrWhiteSpace(version)) return;

      // Show the toolbar on the first install
      if (VsServiceProvider.TryGetService(typeof(DTE2), out object dte))
      {
        var cbs = ((CommandBars)(dte as DTE2).CommandBars);
        CommandBar cb = cbs["Clang Power Tools"];
        cb.Enabled = true;
        cb.Visible = true;
      }
    }

    private async Task RegisterVsServicesAsync()
    {
      // Get DTE service async 
      var dte = await mPackage.GetServiceAsync(typeof(DTE)) as DTE2;
      VsServiceProvider.Register(typeof(DTE2), dte);

      // Get VS Output Window service async
      var vsOutputWindow = await mPackage.GetServiceAsync(typeof(SVsOutputWindow));
      VsServiceProvider.Register(typeof(SVsOutputWindow), vsOutputWindow);

      // Get the status bar service async
      var vsStatusBar = await mPackage.GetServiceAsync(typeof(SVsStatusbar));
      VsServiceProvider.Register(typeof(SVsStatusbar), vsStatusBar);

      // Get Vs Running Document Table service async
      var vsRunningDocumentTable = await mPackage.GetServiceAsync(typeof(SVsRunningDocumentTable));
      VsServiceProvider.Register(typeof(SVsRunningDocumentTable), vsRunningDocumentTable);

      // Get Vs File Change service async
      var vsFileChange = await mPackage.GetServiceAsync(typeof(SVsFileChangeEx));
      VsServiceProvider.Register(typeof(SVsFileChangeEx), vsFileChange);

      // Get VS Solution service async
      var vsSolution = await mPackage.GetServiceAsync(typeof(SVsSolution));
      VsServiceProvider.Register(typeof(SVsSolution), vsSolution);
    }


    private async Task RegisterToEventsAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      RegisterToCPTEvents();
      RegisterToVsEvents();
    }

    private void RegisterToCPTEvents()
    {
      mCommandController.ClangCommandMessageEvent += PowerShellWrapper.mOutputWindowController.Write;
      mCommandController.ClearOutputWindowEvent += PowerShellWrapper.mOutputWindowController.ClearPanel;

      mCommandController.HierarchyDetectedEvent += PowerShellWrapper.mOutputWindowController.OnFileHierarchyDetected;

      mCommandController.HasEncodingErrorEvent += PowerShellWrapper.mOutputWindowController.OnEncodingErrorDetected;
      PowerShellWrapper.mOutputWindowController.HasEncodingErrorEvent += mCommandController.OnEncodingErrorDetected;

      mCommandController.ClearErrorListEvent += mErrorWindowController.OnClangCommandBegin;

      CompileCommand.Instance.HierarchyDetectedEvent += mCommandController.OnFileHierarchyChanged;
      TidyCommand.Instance.HierarchyDetectedEvent += mCommandController.OnFileHierarchyChanged;

      mCommandController.ErrorDetectedEvent += PowerShellWrapper.mOutputWindowController.OnErrorDetected;
      PowerShellWrapper.mOutputWindowController.ErrorDetectedEvent += mErrorWindowController.OnErrorDetected;

      PowerShellWrapper.mOutputWindowController.JsonCompilationDbFilePathEvent += JsonCompilationDatabaseCommand.Instance.OpenInFileExplorer;


      RunController.CloseDataStreamingEvent += mCommandController.OnAfterRunCommand;
      FormatCommand.Instance.FormatEvent += mCommandController.OnAfterFormatCommand;

      CompileCommand.Instance.ActiveDocumentEvent += mCommandController.OnActiveDocumentCheck;
      TidyCommand.Instance.ActiveDocumentEvent += mCommandController.OnActiveDocumentCheck;

      CompileCommand.Instance.IgnoredItemsEvent += mCommandController.OnItemIgnore;
      TidyCommand.Instance.IgnoredItemsEvent += mCommandController.OnItemIgnore;
      FormatCommand.Instance.IgnoredItemsEvent += mCommandController.OnItemIgnore;

      PowerShellWrapper.DataHandler += PowerShellWrapper.mOutputWindowController.OutputDataReceived;
      PowerShellWrapper.DataErrorHandler += PowerShellWrapper.mOutputWindowController.OutputDataErrorReceived;
      PowerShellWrapper.ExitedHandler += PowerShellWrapper.mOutputWindowController.ClosedDataConnection;
      PowerShellWrapper.ExitedHandler += GenerateDocumentation.ClosedDataConnection;
    }

    private void RegisterToVsEvents()
    {
      if (null != mBuildEvents)
      {
        mBuildEvents.OnBuildBegin += mErrorWindowController.OnBuildBegin;
        mBuildEvents.OnBuildBegin += mCommandController.OnMSVCBuildBegin;
        mBuildEvents.OnBuildDone += mCommandController.OnMSVCBuildDone;
      }

      if (null != mCommandEvents)
        mCommandEvents.BeforeExecute += mCommandController.CommandEventsBeforeExecute;

      if (null != CommandControllerInstance.CommandController.mRunningDocTableEvents)
        CommandControllerInstance.CommandController.mRunningDocTableEvents.BeforeSave += mCommandController.OnBeforeSave;

      if (null != CommandControllerInstance.CommandController.mRunningDocTableEvents)
        CommandControllerInstance.CommandController.mRunningDocTableEvents.AfterSave += mCommandController.OnAfterSave;

      if (null != mDteEvents)
        mDteEvents.OnBeginShutdown += UnregisterFromEvents;

      if (windowEvents != null)
        windowEvents.WindowActivated += mCommandController.OnWindowActivated;
    }

    private void UnregisterFromEvents()
    {
      UnregisterFromCPTEvents();
      UnregisterFromVsEvents();
    }

    private void UnregisterFromCPTEvents()
    {
      mCommandController.ClangCommandMessageEvent -= PowerShellWrapper.mOutputWindowController.Write;
      mCommandController.ClearOutputWindowEvent -= PowerShellWrapper.mOutputWindowController.ClearPanel;

      mCommandController.HierarchyDetectedEvent -= PowerShellWrapper.mOutputWindowController.OnFileHierarchyDetected;

      mCommandController.HasEncodingErrorEvent -= PowerShellWrapper.mOutputWindowController.OnEncodingErrorDetected;
      PowerShellWrapper.mOutputWindowController.HasEncodingErrorEvent -= mCommandController.OnEncodingErrorDetected;

      mCommandController.ClearErrorListEvent -= mErrorWindowController.OnClangCommandBegin;

      CompileCommand.Instance.HierarchyDetectedEvent -= mCommandController.OnFileHierarchyChanged;
      TidyCommand.Instance.HierarchyDetectedEvent -= mCommandController.OnFileHierarchyChanged;

      mCommandController.ErrorDetectedEvent -= PowerShellWrapper.mOutputWindowController.OnErrorDetected;
      PowerShellWrapper.mOutputWindowController.ErrorDetectedEvent -= mErrorWindowController.OnErrorDetected;

      PowerShellWrapper.mOutputWindowController.JsonCompilationDbFilePathEvent -= JsonCompilationDatabaseCommand.Instance.OpenInFileExplorer;

      RunController.CloseDataStreamingEvent -= mCommandController.OnAfterRunCommand;
      FormatCommand.Instance.FormatEvent -= mCommandController.OnAfterFormatCommand;

      CompileCommand.Instance.ActiveDocumentEvent -= mCommandController.OnActiveDocumentCheck;
      TidyCommand.Instance.ActiveDocumentEvent -= mCommandController.OnActiveDocumentCheck;

      CompileCommand.Instance.IgnoredItemsEvent -= mCommandController.OnItemIgnore;
      TidyCommand.Instance.IgnoredItemsEvent -= mCommandController.OnItemIgnore;
      FormatCommand.Instance.IgnoredItemsEvent -= mCommandController.OnItemIgnore;

      PowerShellWrapper.DataHandler -= PowerShellWrapper.mOutputWindowController.OutputDataReceived;
      PowerShellWrapper.DataErrorHandler -= PowerShellWrapper.mOutputWindowController.OutputDataErrorReceived;
      PowerShellWrapper.ExitedHandler -= PowerShellWrapper.mOutputWindowController.ClosedDataConnection;
      PowerShellWrapper.ExitedHandler -= GenerateDocumentation.ClosedDataConnection;
    }

    private void DeleteTempSolution()
    {
      var solutionPath = Path.Combine(TidyConstants.TempsFolderPath, TidyConstants.SolutionTempGuid);
      if (Directory.Exists(solutionPath))
      {
        Directory.Delete(solutionPath, true);
      }
    }

    /// <summary>
    /// Hide Find Tool Window and Tidy Tool Window
    /// </summary>
    /// <returns></returns>
    private int HideToolWindow()
    {
      var tidyToolWindow = mPackage.FindToolWindow(typeof(TidyToolWindow), 0, false);
      if (tidyToolWindow is null) return VSConstants.S_OK;
      var tidyWindow = tidyToolWindow.Frame as IVsWindowFrame;
      tidyWindow?.Hide();

      var findToolWindow = mPackage.FindToolWindow(typeof(FindToolWindow), 0, false);
      if (findToolWindow is null) return VSConstants.S_OK;
      var findWindow = findToolWindow.Frame as IVsWindowFrame;
      findWindow?.Hide();

      return VSConstants.S_OK;
    }

    private void DeleteCacheReporitory()
    {
      //Delete cache repository;
      if (Directory.Exists(PathConstants.CacheRepositoryPath))
      {
        Directory.Delete(PathConstants.CacheRepositoryPath, true);
      }
    }

    private void DeleteFromFindToolWindowHistory()
    {
      FindToolWindowProvider.RemoveFromFullList();
      FindToolWindowHandler findToolWindowHandler = new FindToolWindowHandler();
      findToolWindowHandler.SaveMatchersHistoryData();
    }

    private void CreateCacheRepository()
    {
      //Create cache repository
      if (Directory.Exists(PathConstants.CacheRepositoryPath))
      {
        DeleteCacheReporitory();
      }
      Directory.CreateDirectory(PathConstants.CacheRepositoryPath);
    }

    private void UnregisterFromVsEvents()
    {
      if (null != mBuildEvents)
      {
        mBuildEvents.OnBuildBegin -= mErrorWindowController.OnBuildBegin;
        mBuildEvents.OnBuildBegin -= mCommandController.OnMSVCBuildBegin;
        mBuildEvents.OnBuildDone -= mCommandController.OnMSVCBuildDone;
      }

      if (null != mCommandEvents)
        mCommandEvents.BeforeExecute -= mCommandController.CommandEventsBeforeExecute;

      if (null != CommandControllerInstance.CommandController.mRunningDocTableEvents)
        CommandControllerInstance.CommandController.mRunningDocTableEvents.BeforeSave -= mCommandController.OnBeforeSave;

      if (null != mDteEvents)
        mDteEvents.OnBeginShutdown -= UnregisterFromEvents;

      if (windowEvents != null)
        windowEvents.WindowActivated -= mCommandController.OnWindowActivated;
    }


    #endregion

  }
}
