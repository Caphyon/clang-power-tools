using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Reflection;
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
  [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
  [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
  [ProvideMenuResource("Menus.ctmenu", 1)]
  [Guid(PackageGuidString)]
  public sealed class RunClangPowerToolsPackage2 : AsyncPackage
  {
    #region Members

    Object mClangPackageImpl;

    /// <summary>
    /// RunPowerShellCommandPackage GUID string.
    /// </summary>
    public const string PackageGuidString = "f564f9d3-01ae-493e-883b-18deebdb975e";

    #endregion


    #region Constructor


    /// <summary>
    /// Initializes a new instance of the <see cref="TidyCommand"/> class.
    /// </summary>
    public RunClangPowerToolsPackage2()
    {

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

      var shellService = (IVsShell)await GetServiceAsync(typeof(SVsShell));
      object releaseVersionObj = "";
      shellService.GetProperty(-9068, out releaseVersionObj);
      string releaseVersion = (string)releaseVersionObj;
      releaseVersion = releaseVersion.Substring(0, releaseVersion.IndexOf('.')) + ".0";
      string assemblyName = new Version(releaseVersion) < new Version("17.0")
          ? "ClangPowerToolsLib16.dll"
          : "ClangPowerToolsLib17.dll";

      string currentDir = Path.GetDirectoryName(GetType().Assembly.Location);
      Assembly assembly = Assembly.LoadFile(Path.Combine(currentDir, assemblyName));

      Type type = assembly.GetType("ClangPowerTools.ClangPowerToolsPackageImpl");
      mClangPackageImpl = Activator.CreateInstance(type, this);
      MethodInfo method = type.GetMethod("InitializeAsync");

      await (Task)method.Invoke(mClangPackageImpl, null);



      await base.InitializeAsync(cancellationToken, progress);
    }

    #endregion

  }
}
