//------------------------------------------------------------------------------
// <copyright file="RunPowerShellCommandPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using EnvDTE;
using EnvDTE80;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.Interop;

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
  [ProvideOptionPage(typeof(GeneralOptions), "Clang Power Tools", "General", 0, 0, true)]
  [ProvideOptionPage(typeof(TidyOptions), "Clang Power Tools\\Tidy", "Options", 0, 0, true)]
  [ProvideOptionPage(typeof(TidyChecks), "Clang Power Tools\\Tidy", "Checks", 0, 0, true)]
  [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
  public sealed class RunClangPowerToolsPackage : Package
  {
    #region Members

    /// <summary>
    /// RunPowerShellCommandPackage GUID string.
    /// </summary>
    public const string PackageGuidString = "f564f9d3-01ae-493e-883b-18deebdb975e";
    public static readonly Guid CommandSet = new Guid("498fdff5-5217-4da9-88d2-edad44ba3874");
    private Dictionary<string, string> mVsVersions = new Dictionary<string, string>
    {
      {"11.0", "2010"},
      {"12.0", "2012"},
      {"13.0", "2013"},
      {"14.0", "2015"},
      {"15.0", "2017"}
    };
    private DTE2 mDte;
    private CommandsController mCommandsController;

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
      mDte = (DTE2)GetService(typeof(DTE));
      mDte.Events.BuildEvents.OnBuildBegin += 
        new _dispBuildEvents_OnBuildBeginEventHandler(this.OnBuildBegin);

      string edition = mDte.Edition;
      mVsVersions.TryGetValue(mDte.Version, out string version);
      mCommandsController = new CommandsController(this, mDte);

      TidyCommand TidyCmd = new TidyCommand(this, CommandSet, CommandIds.kTidyId, mDte,
       edition, version, mCommandsController);

      CompileCommand CompileCmd = new CompileCommand(this, CommandSet, CommandIds.kCompileId, mDte,
        edition, version, mCommandsController);

      SettingsCommand SettingsCmd = new SettingsCommand(this, CommandSet, CommandIds.kSettingsId);
    }

    private void OnBuildBegin(EnvDTE.vsBuildScope Scope, EnvDTE.vsBuildAction Action)
    {
      ErrorsManager errorsManager = new ErrorsManager(this, mDte);
      errorsManager.Clear();
    }

    #endregion
  }
}
