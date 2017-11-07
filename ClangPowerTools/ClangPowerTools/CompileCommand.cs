//------------------------------------------------------------------------------
// <copyright file="RunPowerShellCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.Windows.Forms;
using EnvDTE;
using System.IO;

namespace ClangPowerTools
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class CompileCommand
  {
    #region Members

    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 0x0102;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("498fdff5-5217-4da9-88d2-edad44ba3874");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private Package mPackage;

    private DTE2 mDte;
    private string mVsEdition;
    private string mVsVersion;
    private string kVs15Version = "2017";

    private OutputManager mOutputManager;
    private ErrorsManager mErrorsManager;
    private CommandsController mCommandsController;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CompileCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private CompileCommand(Package aPackage, DTE2 aDte, string aEdition,
      string aVersion, CommandsController aCommandsController)
    {
      this.mPackage = aPackage ?? throw new ArgumentNullException("package");

      mDte = aDte;
      mVsEdition = aEdition;
      mVsVersion = aVersion;
      mCommandsController = aCommandsController;
      mErrorsManager = new ErrorsManager(mPackage, mDte);

      if (this.ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
      {
        var menuCommandID = new CommandID(CommandSet, CommandId);
        var menuCommand = new OleMenuCommand(this.MenuItemCallback, menuCommandID);
        menuCommand.BeforeQueryStatus += mCommandsController.QueryCommandHandler;
        menuCommand.Enabled = true;
        commandService.AddCommand(menuCommand);
      }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static CompileCommand Instance { get; private set; }

    /// <summary>
    /// Gets the service provider from the owner package.
    /// </summary>
    private IServiceProvider ServiceProvider => this.mPackage;

    #endregion

    #region Methods

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static void Initialize(Package aPackage, DTE2 aDte, string aEdition,
      string aVersion, CommandsController aCommandsController)
    {
      Instance = new CompileCommand(aPackage, aDte, aEdition, aVersion, aCommandsController);
    }

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void MenuItemCallback(object sender, EventArgs e)
    {
      mCommandsController.Running = true;
      var task = System.Threading.Tasks.Task.Run(() =>
      {
        try
        {
          GeneralOptions generalOptions = (GeneralOptions)mPackage.GetDialogPage(typeof(GeneralOptions));

          ScriptBuiler scriptBuilder = new ScriptBuiler();
          scriptBuilder.ConstructParameters(generalOptions, null, null, mDte, mVsEdition, mVsVersion);

          ItemsCollector mItemsCollector = new ItemsCollector(mPackage);
          mItemsCollector.CollectSelectedFiles(mDte, ActiveWindowProperties.GetProjectItemOfActiveWindow(mDte));

          mOutputManager = new OutputManager(mDte);
          PowerShellWrapper powerShell = new PowerShellWrapper();
          powerShell.DataHandler += mOutputManager.OutputDataReceived;
          powerShell.DataErrorHandler += mOutputManager.OutputDataErrorReceived;

          if (kVs15Version == mVsVersion)
          {
            Vs15SolutionLoader solutionLoader = new Vs15SolutionLoader(mPackage);
            solutionLoader.EnsureSolutionProjectsAreLoaded();
          }
          mDte.Documents.SaveAll();

          mOutputManager.Clear();
          mOutputManager.Show();
          mOutputManager.AddMessage($"\n{OutputWindowConstants.kStart} {OutputWindowConstants.kComplileCommand}\n");
          foreach (var item in mItemsCollector.GetItems)
          {
            var script = scriptBuilder.GetScript(item, item.GetName());
            powerShell.Invoke(script);
            if (mOutputManager.MissingLlvm)
            {
              mOutputManager.AddMessage(ErrorParserConstants.kMissingLlvmMessage);
              break;
            }
          }
          if (!mOutputManager.EmptyBuffer)
            mOutputManager.AddMessage(String.Join("\n", mOutputManager.Buffer));
          if (!mOutputManager.MissingLlvm)
          {
            mOutputManager.Show();
            mOutputManager.AddMessage($"\n{OutputWindowConstants.kDone} {OutputWindowConstants.kComplileCommand}\n");
          }
          if (mOutputManager.HasErrors)
            mErrorsManager.AddErrors(mOutputManager.Errors);
        }
        catch (Exception exception)
        {
          VsShellUtilities.ShowMessageBox(mPackage, exception.Message, "Error",
            OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
      }).ContinueWith(tsk => mCommandsController.AfterExecute());
    }

    #endregion

  }
}

