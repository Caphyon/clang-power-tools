﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using ClangPowerTools.Commands;
using ClangPowerTools.DialogPages;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.CommandBars;
using EnvDTE;
using EnvDTE80;
using System.Reflection;
using System.Xml;
using System.IO;
using System.Linq;

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
  [PackageRegistration(UseManagedResourcesOnly = true)]
  [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
  [ProvideMenuResource("Menus.ctmenu", 1)]
  [Guid(RunClangPowerToolsPackage.PackageGuidString)]
  [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
  [ProvideOptionPage(typeof(ClangGeneralOptionsView), "Clang Power Tools", "General", 0, 0, true)]
  [ProvideOptionPage(typeof(ClangTidyOptionsView), "Clang Power Tools\\Tidy", "Options", 0, 0, true, Sort = 0)]
  [ProvideOptionPage(typeof(ClangTidyCustomChecksOptionsView), "Clang Power Tools\\Tidy", "Custom Checks", 0, 0, true, Sort = 1)]
  [ProvideOptionPage(typeof(ClangTidyChecksOptionsView), "Clang Power Tools\\Tidy", "Predefined Checks", 0, 0, true, Sort = 2)]
  [ProvideOptionPage(typeof(ClangFormatOptionsView), "Clang Power Tools", "Format", 0, 0, true, Sort = 4)]
  [ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F")]
  public sealed class RunClangPowerToolsPackage : Package, IVsSolutionEvents
  {
    #region Members

    /// <summary>
    /// RunPowerShellCommandPackage GUID string.
    /// </summary>
    public const string PackageGuidString = "f564f9d3-01ae-493e-883b-18deebdb975e";
    public static readonly Guid CommandSet = new Guid("498fdff5-5217-4da9-88d2-edad44ba3874");

    private uint mHSolutionEvents = uint.MaxValue;
    private RunningDocTableEvents mRunningDocTableEvents;
    private IVsSolution mSolution;
    private CommandEvents mCommandEvents;
    private BuildEvents mBuildEvents;

    #region Commands

    private CommandsController mCommandsController  = null;
    private ClangCommand mCompileCmd                = null;
    private ClangCommand mTidyCmd                   = null;
    private ClangCommand mClangFormatCmd            = null;
    private ClangCommand mStopClangCmd              = null;
    private BasicCommand mSettingsCmd               = null;

    #endregion

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

    #region Package Members

    /// <summary>
    /// Initialization of the package; this method is called right after the package is sited, so this is the place
    /// where you can put all the initialization code that rely on services provided by VisualStudio.
    /// </summary>
    protected override void Initialize()
    {
      base.Initialize();

      mRunningDocTableEvents = new RunningDocTableEvents(this);

      //Settings command is always visible
      mSettingsCmd = new SettingsCommand(this, CommandSet, CommandIds.kSettingsId);

      var dte = GetService(typeof(DTE)) as DTE2;

      mBuildEvents = dte.Events.BuildEvents;
      mCommandEvents = dte.Events.CommandEvents;

      var generalOptions = (ClangGeneralOptionsView)this.GetDialogPage(typeof(ClangGeneralOptionsView));
      if (null == generalOptions.Version || string.IsNullOrWhiteSpace(generalOptions.Version))
        ShowToolbare(dte); // Show the toolbar on the first install

      AdviseSolutionEvents();
    }

    #endregion

    #region Get Pointer to IVsSolutionEvents

    private void AdviseSolutionEvents()
    {
      try
      {
        UnadviseSolutionEvents();
        mSolution = GetService(typeof(SVsSolution)) as IVsSolution;
        mSolution?.AdviseSolutionEvents(this, out mHSolutionEvents);
      }
      catch (Exception)
      {
      }
    }

    private void UnadviseSolutionEvents()
    {
      if (null == mSolution)
        return;
      if (uint.MaxValue != mHSolutionEvents)
      {
        mSolution.UnadviseSolutionEvents(mHSolutionEvents);
        mHSolutionEvents = uint.MaxValue;
      }
      mSolution = null;
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
      try
      {
        var dte = GetService(typeof(DTE)) as DTE2;
        mCommandsController = new CommandsController(this, dte);

        if (null == mTidyCmd)
          mTidyCmd = new TidyCommand(this, CommandSet, CommandIds.kTidyId, mCommandsController);

        if (null == mCompileCmd)
          mCompileCmd = new CompileCommand(this, CommandSet, CommandIds.kCompileId, mCommandsController);

        if (null == mClangFormatCmd)
          mClangFormatCmd = new ClangFormatCommand(this, CommandSet, CommandIds.kClangFormat, mCommandsController);

        if (null == mStopClangCmd)
          mStopClangCmd = new StopClang(this, CommandSet, CommandIds.kStopClang, mCommandsController);

        DispatcherHandler.Initialize(dte);
        StatusBarHandler.Initialize(this);

        var generalOptions = (ClangGeneralOptionsView)this.GetDialogPage(typeof(ClangGeneralOptionsView));
        var currentVersion = GetPackageVersion();

        if (0 != string.Compare(generalOptions.Version, currentVersion))
        {
          OutputManager outputManager = new OutputManager(dte);
          outputManager.Show();
          outputManager.AddMessage($"🎉\tClang Power Tools was upgraded to v{currentVersion}\n" +
            $"\tCheck out what's new at http://www.clangpowertools.com/CHANGELOG");

          generalOptions.Version = currentVersion;
          generalOptions.SaveSettingsToStorage();
        }

        mBuildEvents.OnBuildBegin += mCommandsController.OnBuildBegin;
        mBuildEvents.OnBuildDone += mCommandsController.OnBuildDone;

        mBuildEvents.OnBuildDone += mCompileCmd.OnBuildDone;

        mCommandEvents.BeforeExecute += mCompileCmd.CommandEventsBeforeExecute;
        mCommandEvents.BeforeExecute += mTidyCmd.CommandEventsBeforeExecute;

        mRunningDocTableEvents.BeforeSave += mTidyCmd.OnBeforeSave;
        mRunningDocTableEvents.BeforeSave += mClangFormatCmd.OnBeforeSave;
      }
      catch (Exception)
      {
      }

      return VSConstants.S_OK;
    }

  
    public int OnQueryCloseSolution(object aPUnkReserved, ref int aPfCancel)
    {
      return VSConstants.S_OK;
    }

    public int OnBeforeCloseSolution(object aPUnkReserved)
    {
      mBuildEvents.OnBuildBegin += mCommandsController.OnBuildBegin;
      mBuildEvents.OnBuildDone += mCommandsController.OnBuildDone;

      mBuildEvents.OnBuildDone -= mCompileCmd.OnBuildDone;

      mCommandEvents.BeforeExecute -= mCompileCmd.CommandEventsBeforeExecute;
      mCommandEvents.BeforeExecute -= mTidyCmd.CommandEventsBeforeExecute;

      mRunningDocTableEvents.BeforeSave -= mTidyCmd.OnBeforeSave;
      mRunningDocTableEvents.BeforeSave -= mClangFormatCmd.OnBeforeSave;

      return VSConstants.S_OK;
    }

    public int OnAfterCloseSolution(object aPUnkReserved)
    {
      return VSConstants.S_OK;
    }

    #endregion

    #region Private Methods

    public string GetPackageVersion()
    {
      var assemblyPath = Assembly.GetExecutingAssembly().Location;
      assemblyPath = assemblyPath.Substring(0, assemblyPath.LastIndexOf('\\'));
      var manifestPath = Path.Combine(assemblyPath, "extension.vsixmanifest");

      var doc = new XmlDocument();
      doc.Load(manifestPath);
      var metaData = doc.DocumentElement.ChildNodes.Cast<XmlElement>().First(x => x.Name == "Metadata");
      var identity = metaData.ChildNodes.Cast<XmlElement>().First(x => x.Name == "Identity");

      return identity.GetAttribute("Version");
    }

    private void ShowToolbare(DTE2 aDte)
    {
      var cbs = ((CommandBars)aDte.CommandBars);
      CommandBar cb = cbs["Clang Power Tools"];
      cb.Visible = true;
    }

    #endregion

  }
}
