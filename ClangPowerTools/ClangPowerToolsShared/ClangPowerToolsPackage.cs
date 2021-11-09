using ClangPowerTools.Commands;
using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Views;
using ClangPowerTools.Output;
using ClangPowerTools.Services;
using ClangPowerToolsShared.MVVM.Views.ToolWindows;
using ClangPowerToolsShared.MVVM.Constants;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools
{
  /// <summary>
  /// This is the class that implements the package exposed by this assembly.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The minimum requirement for a class to be considered a valid package for Visual Studio
  /// is to implement the IVsPackage interface and register itself with the shell.
  /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
  /// to do it: it derives from the Package class that provides the implementation of the
  /// IVsPackage interface and uses the registration attributes defined in the framework to
  /// register itself and its components with the shell. These attributes tell the pkgdef creation
  /// utility what data to put into .pkgdef file.
  /// </para>
  /// <para>
  /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
  /// </para>
  /// </remarks>
  [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
  [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
  [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
  [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
  [ProvideMenuResource("Menus.ctmenu", 1)]
  [ProvideToolWindow(typeof(TidyToolWindow), Style = VsDockStyle.Tabbed, DockedWidth = 300, Window = "DocumentWell", Orientation = ToolWindowOrientation.Left, Transient = true)]
  [Guid(PackageGuidString)]
  public sealed class RunClangPowerToolsPackage : AsyncPackage, IVsSolutionEvents, IVsSolutionLoadEvents, IVsSolutionEvents7
  {
    #region Members

    /// <summary>
    /// RunPowerShellCommandPackage GUID string.
    /// </summary>
    public const string PackageGuidString = "f564f9d3-01ae-493e-883b-18deebdb975e";

    private uint mHSolutionEvents = uint.MaxValue;
    private RunningDocTableEvents mRunningDocTableEvents;
    private ErrorWindowController mErrorWindowController;
    private OutputWindowController mOutputWindowController;
    private CommandController mCommandController;

    private CommandEvents mCommandEvents;
    private BuildEvents mBuildEvents;
    private DTEEvents mDteEvents;
    private WindowEvents windowEvents;

    #endregion


    #region Constructor


    /// <summary>
    /// Initializes a new instance of the <see cref="TidyCommand"/> class.
    /// </summary>
    public RunClangPowerToolsPackage()
    {
      // Inside this method you can place any initialization code that does not require
      // any Visual Studio service because at this point the package object is created but
      // not sited yet inside Visual Studio environment. The place to do all the other
      // initialization is the Initialize method.
    }

    #endregion


    #region Initialize Package


    /// <summary>
    /// Initialization of the package; this method is called right after the package is sited, so this is the place
    /// where you can put all the initialization code that rely on services provided by VisualStudio.
    /// </summary>
    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
      // Switches to the UI thread in order to consume some services used in command initialization
      await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

      await RegisterVsServicesAsync();

      mCommandController = new CommandController(this);

      CommandControllerInstance.CommandController = mCommandController;

      var vsOutputWindow = VsServiceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;

      mOutputWindowController = new OutputWindowController();
      mOutputWindowController.Initialize(this, vsOutputWindow);

      mRunningDocTableEvents = new RunningDocTableEvents(this);
      mErrorWindowController = new ErrorWindowController(this);

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

      var settingsHandler = new SettingsHandler();
      settingsHandler.InitializeSettings();
      await settingsHandler.InitializeAccountSettingsAsync();

      string version = SettingsProvider.GeneralSettingsModel.Version;

      ShowToolbar(version);
      UpdateVersion(version);

      await mCommandController.InitializeCommandsAsync(this);

      await RegisterToEventsAsync();

      await base.InitializeAsync(cancellationToken, progress);
    }

    //public override IVsAsyncToolWindowFactory GetAsyncToolWindowFactory(Guid toolWindowType)
    //{
    //  return toolWindowType.Equals(Guid.Parse(TidyToolWindow.WindowGuidString)) ? this : null;
    //}

    protected override string GetToolWindowTitle(Type toolWindowType, int id)
    {
      return toolWindowType == typeof(TidyToolWindow) ? TidyToolWindow.Title : base.GetToolWindowTitle(toolWindowType, id);
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
      DeleteCacheReporitory();
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
      var tidyToolWindow = FindToolWindow(typeof(TidyToolWindow), 0, false);
      if (tidyToolWindow is null) return VSConstants.S_OK;
      var window = tidyToolWindow.Frame as IVsWindowFrame;
      window.Hide();
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
      var dte = await GetServiceAsync(typeof(DTE)) as DTE2;
      VsServiceProvider.Register(typeof(DTE2), dte);

      // Get VS Output Window service async
      var vsOutputWindow = await GetServiceAsync(typeof(SVsOutputWindow));
      VsServiceProvider.Register(typeof(SVsOutputWindow), vsOutputWindow);

      // Get the status bar service async
      var vsStatusBar = await GetServiceAsync(typeof(SVsStatusbar));
      VsServiceProvider.Register(typeof(SVsStatusbar), vsStatusBar);

      // Get Vs Running Document Table service async
      var vsRunningDocumentTable = await GetServiceAsync(typeof(SVsRunningDocumentTable));
      VsServiceProvider.Register(typeof(SVsRunningDocumentTable), vsRunningDocumentTable);

      // Get Vs File Change service async
      var vsFileChange = await GetServiceAsync(typeof(SVsFileChangeEx));
      VsServiceProvider.Register(typeof(SVsFileChangeEx), vsFileChange);

      // Get VS Solution service async
      var vsSolution = await GetServiceAsync(typeof(SVsSolution));
      VsServiceProvider.Register(typeof(SVsSolution), vsSolution);
    }


    private async Task RegisterToEventsAsync()
    {
      //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async delegate
      {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        RegisterToCPTEvents();
        RegisterToVsEvents();
      });
    }

    private void RegisterToCPTEvents()
    {
      mCommandController.ClangCommandMessageEvent += mOutputWindowController.Write;
      mCommandController.ClearOutputWindowEvent += mOutputWindowController.ClearPanel;

      mCommandController.HierarchyDetectedEvent += mOutputWindowController.OnFileHierarchyDetected;

      mCommandController.HasEncodingErrorEvent += mOutputWindowController.OnEncodingErrorDetected;
      mOutputWindowController.HasEncodingErrorEvent += mCommandController.OnEncodingErrorDetected;

      mCommandController.ClearErrorListEvent += mErrorWindowController.OnClangCommandBegin;

      CompileCommand.Instance.HierarchyDetectedEvent += mCommandController.OnFileHierarchyChanged;
      TidyCommand.Instance.HierarchyDetectedEvent += mCommandController.OnFileHierarchyChanged;

      mCommandController.ErrorDetectedEvent += mOutputWindowController.OnErrorDetected;
      mOutputWindowController.ErrorDetectedEvent += mErrorWindowController.OnErrorDetected;

      mOutputWindowController.JsonCompilationDbFilePathEvent += JsonCompilationDatabaseCommand.Instance.OpenInFileExplorer;

      CompileCommand.Instance.CloseDataStreamingEvent += mCommandController.OnAfterRunCommand;
      TidyCommand.Instance.CloseDataStreamingEvent += mCommandController.OnAfterRunCommand;
      JsonCompilationDatabaseCommand.Instance.CloseDataStreamingEvent += mCommandController.OnAfterRunCommand;
      FormatCommand.Instance.FormatEvent += mCommandController.OnAfterFormatCommand;

      CompileCommand.Instance.ActiveDocumentEvent += mCommandController.OnActiveDocumentCheck;
      TidyCommand.Instance.ActiveDocumentEvent += mCommandController.OnActiveDocumentCheck;

      CompileCommand.Instance.IgnoredItemsEvent += mCommandController.OnItemIgnore;
      TidyCommand.Instance.IgnoredItemsEvent += mCommandController.OnItemIgnore;
      FormatCommand.Instance.IgnoredItemsEvent += mCommandController.OnItemIgnore;

      PowerShellWrapper.DataHandler += mOutputWindowController.OutputDataReceived;
      PowerShellWrapper.DataErrorHandler += mOutputWindowController.OutputDataErrorReceived;
      PowerShellWrapper.ExitedHandler += mOutputWindowController.ClosedDataConnection;
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

      if (null != mRunningDocTableEvents)
        mRunningDocTableEvents.BeforeSave += mCommandController.OnBeforeSave;

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
      mCommandController.ClangCommandMessageEvent -= mOutputWindowController.Write;
      mCommandController.ClearOutputWindowEvent -= mOutputWindowController.ClearPanel;

      mCommandController.HierarchyDetectedEvent -= mOutputWindowController.OnFileHierarchyDetected;

      mCommandController.HasEncodingErrorEvent -= mOutputWindowController.OnEncodingErrorDetected;
      mOutputWindowController.HasEncodingErrorEvent -= mCommandController.OnEncodingErrorDetected;

      mCommandController.ClearErrorListEvent -= mErrorWindowController.OnClangCommandBegin;

      CompileCommand.Instance.HierarchyDetectedEvent -= mCommandController.OnFileHierarchyChanged;
      TidyCommand.Instance.HierarchyDetectedEvent -= mCommandController.OnFileHierarchyChanged;

      mCommandController.ErrorDetectedEvent -= mOutputWindowController.OnErrorDetected;
      mOutputWindowController.ErrorDetectedEvent -= mErrorWindowController.OnErrorDetected;

      mOutputWindowController.JsonCompilationDbFilePathEvent -= JsonCompilationDatabaseCommand.Instance.OpenInFileExplorer;

      CompileCommand.Instance.CloseDataStreamingEvent -= mCommandController.OnAfterRunCommand;
      TidyCommand.Instance.CloseDataStreamingEvent -= mCommandController.OnAfterRunCommand;
      JsonCompilationDatabaseCommand.Instance.CloseDataStreamingEvent += mCommandController.OnAfterRunCommand;
      FormatCommand.Instance.FormatEvent -= mCommandController.OnAfterFormatCommand;

      CompileCommand.Instance.ActiveDocumentEvent -= mCommandController.OnActiveDocumentCheck;
      TidyCommand.Instance.ActiveDocumentEvent -= mCommandController.OnActiveDocumentCheck;

      CompileCommand.Instance.IgnoredItemsEvent -= mCommandController.OnItemIgnore;
      TidyCommand.Instance.IgnoredItemsEvent -= mCommandController.OnItemIgnore;
      FormatCommand.Instance.IgnoredItemsEvent -= mCommandController.OnItemIgnore;

      PowerShellWrapper.DataHandler -= mOutputWindowController.OutputDataReceived;
      PowerShellWrapper.DataErrorHandler -= mOutputWindowController.OutputDataErrorReceived;
      PowerShellWrapper.ExitedHandler -= mOutputWindowController.ClosedDataConnection;
    }

    private void DeleteTempSolution()
    {
      var solutionPath = Path.Combine(TidyConstants.TempsFolderPath, TidyConstants.SolutionTempGuid);
      if (Directory.Exists(solutionPath))
      {
        Directory.Delete(TidyConstants.LongFilePrefix + solutionPath, true);
      }
    }

    private int HideToolWindow()
    {
      var tidyToolWindow = FindToolWindow(typeof(TidyToolWindow), 0, false);
      if (tidyToolWindow is null) return VSConstants.S_OK;
      var window = tidyToolWindow.Frame as IVsWindowFrame;
      window.Hide();
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

      if (null != mRunningDocTableEvents)
        mRunningDocTableEvents.BeforeSave -= mCommandController.OnBeforeSave;

      if (null != mDteEvents)
        mDteEvents.OnBeginShutdown -= UnregisterFromEvents;

      if (windowEvents != null)
        windowEvents.WindowActivated -= mCommandController.OnWindowActivated;
    }

    #endregion

  }
}
