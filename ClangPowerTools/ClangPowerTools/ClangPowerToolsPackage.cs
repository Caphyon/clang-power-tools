using ClangPowerTools.Commands;
using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Controllers;
using ClangPowerTools.Output;
using ClangPowerTools.Services;
using ClangPowerTools.Tests;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
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
  [ProvideMenuResource("Menus.ctmenu", 1)]
  [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
  [ProvideOptionPage(typeof(ClangGeneralOptionsView), "Clang Power Tools", "General", 0, 0, true)]
  [ProvideOptionPage(typeof(ClangTidyOptionsView), "Clang Power Tools\\Tidy", "Options", 0, 0, true, Sort = 0)]
  [ProvideOptionPage(typeof(ClangTidyCustomChecksOptionsView), "Clang Power Tools\\Tidy", "Custom Checks", 0, 0, true, Sort = 1)]
  [ProvideOptionPage(typeof(ClangTidyPredefinedChecksOptionsView), "Clang Power Tools\\Tidy", "Predefined Checks", 0, 0, true, Sort = 2)]
  [ProvideOptionPage(typeof(ClangFormatOptionsView), "Clang Power Tools", "Format", 0, 0, true, Sort = 4)]
  [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
  [ProvideMenuResource("Menus.ctmenu", 1)]
  [Guid(RunClangPowerToolsPackage.PackageGuidString)]
  public sealed class RunClangPowerToolsPackage : AsyncPackage, IVsSolutionEvents
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
    private LicenseController mLicenseController;

    private CommandEvents mCommandEvents;
    private BuildEvents mBuildEvents;
    private DTEEvents mDteEvents;

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
      CommandTestUtility.CommandController = mCommandController;

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

      // Get the build and command events from DTE
      if (VsServiceProvider.TryGetService(typeof(DTE), out object dte))
      {
        var dte2 = dte as DTE2;
        mBuildEvents = dte2.Events.BuildEvents;
        mCommandEvents = dte2.Events.CommandEvents;
        mDteEvents = dte2.Events.DTEEvents;
      }

      DispatcherHandler.Initialize(dte as DTE2);
      SettingsProvider.Initialize(this);

      // Detect the first install 
      if (string.IsNullOrWhiteSpace(SettingsProvider.GeneralSettings.Version))
        ShowToolbare(); // Show the toolbar on the first install

      var currentVersion = PackageUtility.GetVersion();
      if (!string.IsNullOrWhiteSpace(currentVersion) &&
        0 > string.Compare(SettingsProvider.GeneralSettings.Version, currentVersion))
      {
        mOutputWindowController.Clear();
        mOutputWindowController.Show();
        mOutputWindowController.Write($"🎉\tClang Power Tools was upgraded to v{currentVersion}\n" +
          $"\tCheck out what's new at http://www.clangpowertools.com/CHANGELOG");

        SettingsProvider.GeneralSettings.Version = currentVersion;
      }
      SettingsHandler.SaveGeneralSettings();

      await mCommandController.InitializeCommandsAsync(this);
      mLicenseController = new LicenseController();

      RegisterToEvents();
      await mLicenseController.CheckLicenseAsync();

      await base.InitializeAsync(cancellationToken, progress);
    }

    #endregion


    #region Get Pointer to IVsSolutionEvents


    private void AdviseSolutionEvents(IVsSolution aVsSolution)
    {
      try
      {
        aVsSolution?.AdviseSolutionEvents(this, out mHSolutionEvents);
      }
      catch (Exception)
      {
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
      return VSConstants.S_OK;
    }

    public int OnQueryCloseProject(IVsHierarchy aPHierarchy, int aFRemoving, ref int aPfCancel)
    {
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
      return VSConstants.S_OK;
    }

    public int OnQueryUnloadProject(IVsHierarchy aPRealHierarchy, ref int aPfCancel)
    {
      return VSConstants.S_OK;
    }

    public int OnBeforeUnloadProject(IVsHierarchy aPRealHierarchy, IVsHierarchy aPStubHierarchy)
    {
      return VSConstants.S_OK;
    }


    public int OnAfterOpenSolution(object aPUnkReserved, int aFNewSolution)
    {
      return VSConstants.S_OK;
    }


    public int OnQueryCloseSolution(object aPUnkReserved, ref int aPfCancel)
    {
      return VSConstants.S_OK;
    }

    public int OnBeforeCloseSolution(object aPUnkReserved)
    {
      return VSConstants.S_OK;
    }

    public int OnAfterCloseSolution(object aPUnkReserved)
    {
      return VSConstants.S_OK;
    }


    #endregion


    #region Private Methods

    private async Task RegisterVsServicesAsync()
    {
      // Get DTE service async 
      var dte = await GetServiceAsync(typeof(DTE)) as DTE2;
      VsServiceProvider.Register(typeof(DTE), dte);

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


    private void RegisterToEvents()
    {
      RegisterToCPTEvents();
      RegisterToVsEvents();
    }

    private void RegisterToCPTEvents()
    {
      mCommandController.ClangCommandMessageEvent += mOutputWindowController.Write;
      mCommandController.HierarchyDetectedEvent += mOutputWindowController.OnFileHierarchyDetected;

      mCommandController.ClearErrorListEvent += mErrorWindowController.OnClangCommandBegin;

      mCommandController.MissingLlvmEvent += CompileCommand.Instance.OnMissingLLVMDetected;
      mCommandController.MissingLlvmEvent += TidyCommand.Instance.OnMissingLLVMDetected;

      CompileCommand.Instance.HierarchyDetectedEvent += mCommandController.OnFileHierarchyChanged;
      TidyCommand.Instance.HierarchyDetectedEvent += mCommandController.OnFileHierarchyChanged;

      mOutputWindowController.ErrorDetectedEvent += mErrorWindowController.OnErrorDetected;
      mOutputWindowController.MissingLlvmEvent += mCommandController.OnMissingLLVMDetected;

      CompileCommand.Instance.CloseDataStreamingEvent += mCommandController.OnAfterRunCommand;
      TidyCommand.Instance.CloseDataStreamingEvent += mCommandController.OnAfterRunCommand;
      FormatCommand.Instance.CloseDataStreamingEvent += mCommandController.OnAfterRunCommand;

      CompileCommand.Instance.ActiveDocumentEvent += mCommandController.OnActiveDocumentCheck;
      TidyCommand.Instance.ActiveDocumentEvent += mCommandController.OnActiveDocumentCheck;
      FormatCommand.Instance.ActiveDocumentEvent += mCommandController.OnActiveDocumentCheck;

      PowerShellWrapper.DataHandler += mOutputWindowController.OutputDataReceived;
      PowerShellWrapper.DataErrorHandler += mOutputWindowController.OutputDataErrorReceived;
      PowerShellWrapper.ExitedHandler += mOutputWindowController.ClosedDataConnection;

      AccountController.OnLicenseStatusChanced += mCommandController.OnLicenseChanged;
      LicenseController.OnLicenseStatusChanced += mCommandController.OnLicenseChanged;
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
      {
        mCommandEvents.BeforeExecute += mCommandController.CommandEventsBeforeExecute;
      }

      if (null != mRunningDocTableEvents)
      {
        mRunningDocTableEvents.BeforeSave += mCommandController.OnBeforeSave;
      }

      if (null != mDteEvents)
      {
        mDteEvents.OnBeginShutdown += UnregisterFromEvents;
        mDteEvents.OnBeginShutdown += UnregisterFromCPTEvents;
      }
    }

    private void UnregisterFromEvents()
    {
      UnregisterFromCPTEvents();
      UnregisterFromVsEvents();
    }

    private void UnregisterFromCPTEvents()
    {
      mCommandController.ClangCommandMessageEvent -= mOutputWindowController.Write;
      mCommandController.HierarchyDetectedEvent -= mOutputWindowController.OnFileHierarchyDetected;

      mCommandController.ClearErrorListEvent -= mErrorWindowController.OnClangCommandBegin;

      mCommandController.MissingLlvmEvent -= CompileCommand.Instance.OnMissingLLVMDetected;
      mCommandController.MissingLlvmEvent -= TidyCommand.Instance.OnMissingLLVMDetected;

      CompileCommand.Instance.HierarchyDetectedEvent -= mCommandController.OnFileHierarchyChanged;
      TidyCommand.Instance.HierarchyDetectedEvent -= mCommandController.OnFileHierarchyChanged;

      mOutputWindowController.ErrorDetectedEvent -= mErrorWindowController.OnErrorDetected;
      mOutputWindowController.MissingLlvmEvent -= mCommandController.OnMissingLLVMDetected;

      CompileCommand.Instance.CloseDataStreamingEvent -= mCommandController.OnAfterRunCommand;
      TidyCommand.Instance.CloseDataStreamingEvent -= mCommandController.OnAfterRunCommand;
      FormatCommand.Instance.CloseDataStreamingEvent -= mCommandController.OnAfterRunCommand;

      CompileCommand.Instance.ActiveDocumentEvent -= mCommandController.OnActiveDocumentCheck;
      TidyCommand.Instance.ActiveDocumentEvent -= mCommandController.OnActiveDocumentCheck;
      FormatCommand.Instance.ActiveDocumentEvent -= mCommandController.OnActiveDocumentCheck;

      PowerShellWrapper.DataHandler -= mOutputWindowController.OutputDataReceived;
      PowerShellWrapper.DataErrorHandler -= mOutputWindowController.OutputDataErrorReceived;
      PowerShellWrapper.ExitedHandler -= mOutputWindowController.ClosedDataConnection;

      AccountController.OnLicenseStatusChanced -= mCommandController.OnLicenseChanged;
      LicenseController.OnLicenseStatusChanced -= mCommandController.OnLicenseChanged;
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
    }


    private void ShowToolbare()
    {
      if (VsServiceProvider.TryGetService(typeof(DTE), out object dte))
      {
        var cbs = ((CommandBars)(dte as DTE2).CommandBars);
        CommandBar cb = cbs["Clang Power Tools"];
        cb.Visible = true;
      }
    }

    #endregion

  }
}
