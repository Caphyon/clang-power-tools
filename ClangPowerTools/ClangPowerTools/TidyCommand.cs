//------------------------------------------------------------------------------
// <copyright file="RunPowerShellCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using System.IO;

namespace ClangPowerTools
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class TidyCommand
  {
    #region Members

    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 0x0101;

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
    private FileChangerWatcher mFileWatcher;
    private FileOpener mFileOpener;
    private CommandsController mCommandsController;
    private ItemsCollector mItemsCollector;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="TidyCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>

    private TidyCommand(Package aPackage, DTE2 aDte, string aEdition, 
      string aVersion, CommandsController aCommandsController)
    {
      mPackage = aPackage ?? throw new ArgumentNullException("package");

      mDte = aDte;
      mVsEdition = aEdition;
      mVsVersion = aVersion;
      mCommandsController = aCommandsController;
      mErrorsManager = new ErrorsManager(mPackage, mDte);
      mFileOpener = new FileOpener(mDte);

      if (this.ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
      {
        var menuCommandID = new CommandID(CommandSet, CommandId);
        var menuItem = new OleMenuCommand(this.MenuItemCallback, menuCommandID);
        menuItem.BeforeQueryStatus += mCommandsController.QueryCommandHandler;
        commandService.AddCommand(menuItem);
      }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static TidyCommand Instance { get; private set; }

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
      Instance = new TidyCommand(aPackage, aDte, aEdition, aVersion, aCommandsController);
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
      System.Threading.Tasks.Task.Run(() =>
      {
        GeneralOptions generalOptions = (GeneralOptions)mPackage.GetDialogPage(typeof(GeneralOptions));
        TidyOptions tidyPage = (TidyOptions)mPackage.GetDialogPage(typeof(TidyOptions));

        ScriptBuiler scriptBuilder = new ScriptBuiler();
        scriptBuilder.ConstructParameters(generalOptions, tidyPage, mVsEdition, mVsVersion);

        mItemsCollector = new ItemsCollector(mPackage);
        mItemsCollector.CollectSelectedFiles(mDte);

        mOutputManager = new OutputManager(mDte);
        PowerShellWrapper powerShell = new PowerShellWrapper();
        powerShell.DataHandler += mOutputManager.OutputDataReceived;
        powerShell.DataErrorHandler += mOutputManager.OutputDataErrorReceived;

        mFileWatcher = new FileChangerWatcher();

        try
        {
          mDte.Documents.SaveAll();
          if (kVs15Version == mVsVersion)
          {
            Vs15SolutionLoader solutionLoader = new Vs15SolutionLoader(mPackage);
            solutionLoader.EnsureSolutionProjectsAreLoaded();
          }
          using (var guard = new SilentFileChangerGuard())
          {
            if(tidyPage.Fix)
            {
              WatchFiles();
              SilentFiles(guard);
            }

            mOutputManager.Clear();
            mOutputManager.Show();
            mOutputManager.AddMessage($"\n{OutputWindowConstants.kStart} {OutputWindowConstants.kTidyCodeCommand}\n");
            foreach (var item in mItemsCollector.GetItems)
            {
              string script = scriptBuilder.GetScript(item.Item1, item.Item1.GetName());
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
              mOutputManager.AddMessage($"\n{OutputWindowConstants.kDone} {OutputWindowConstants.kTidyCodeCommand}\n");
            }
            if (mOutputManager.HasErrors)
              mErrorsManager.AddErrors(mOutputManager.Errors);
          }
        }
        catch (Exception exception)
        {
          VsShellUtilities.ShowMessageBox(mPackage, exception.Message, "Error",
            OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
      }).ContinueWith(tsk => mCommandsController.AfterExecute()); ;
    }

    #endregion

    #region Helpers

    private void SilentFiles(SilentFileChangerGuard aGuard)
    {
      FilePathCollector fileCollector = new FilePathCollector();
      fileCollector.Collect(mItemsCollector.GetItems);

      // silent all open files
      foreach (Document doc in mDte.Documents)
        aGuard.Add(new SilentFileChanger(mPackage, Path.Combine(doc.Path, doc.Name), true));
      //silent all selected files
      aGuard.AddRange(mPackage, fileCollector.Files);
    }

    private void WatchFiles()
    {
      mFileWatcher.OnChanged += mFileOpener.FileChanged;
      string solutionFolderPath = mDte.Solution.FullName
        .Substring(0, mDte.Solution.FullName.LastIndexOf('\\'));
      mFileWatcher.Run(solutionFolderPath);
    }

    #endregion

  }
}
