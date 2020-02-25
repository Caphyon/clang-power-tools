using ClangPowerTools.CMake;
using ClangPowerTools.Commands;
using ClangPowerTools.Events;
using ClangPowerTools.Helpers;
using ClangPowerTools.Handlers;
using ClangPowerTools.MVVM.Views;
using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using Task = System.Threading.Tasks.Task;
using ClangPowerTools.MVVM.Controllers;
using ClangPowerTools.Error;
using System.Windows.Forms;
using System.Threading;

namespace ClangPowerTools
{
  /// <summary>
  /// Contains all the logic of disable and enable the clang custom commands  
  /// </summary>
  public class CommandController
  {
    #region Members

    public bool running = false;
    public bool vsBuildRunning = false;
    public bool activeAccount = false;
    public bool tokenExists = false;
    public bool clearOutputOnFormat = false;

    public static readonly Guid mCommandSet = new Guid("498fdff5-5217-4da9-88d2-edad44ba3874");

    public event EventHandler<VsHierarchyDetectedEventArgs> HierarchyDetectedEvent;
    public event EventHandler<ClangCommandMessageEventArgs> ClangCommandMessageEvent;
    public event EventHandler<ClearEventArgs> ClearErrorListEvent;
    public event EventHandler<ClearEventArgs> ClearOutputWindowEvent;
    public event EventHandler<EventArgs> ErrorDetectedEvent;
    public event EventHandler<EventArgs> HasEncodingErrorEvent;

    private readonly SettingsProvider settingsProvider = new SettingsProvider();
    private readonly Commands2 mCommand;
    private CommandUILocation commandUILocation;
    private int currentCommand;
    private bool mSaveCommandWasGiven = false;
    private bool mFormatAfterTidyFlag = false;
    private bool isActiveDocument = true;
    static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

    #endregion

    #region Constructor

    public CommandController(AsyncPackage aAsyncPackage)
    {
      if (VsServiceProvider.TryGetService(typeof(DTE), out object dte))
      {
        var dte2 = (DTE2)dte;
        mCommand = (Commands2)dte2.Commands;
      }
    }

    #endregion

    #region Public Methods

    public async Task InitializeCommandsAsync(AsyncPackage aAsyncPackage)
    {
      if (CompileCommand.Instance == null)
      {
        await CompileCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kCompileId);
        await CompileCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kCompileToolbarId);
      }

      if (TidyCommand.Instance == null)
      {
        await TidyCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kTidyId);
        await TidyCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kTidyToolbarId);
        await TidyCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kTidyFixId);
        await TidyCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kTidyFixToolbarId);
      }

      if (FormatCommand.Instance == null)
      {
        await FormatCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kClangFormat);
        await FormatCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kClangFormatToolbarId);
      }

      if (IgnoreFormatCommand.Instance == null)
      {
        await IgnoreFormatCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kIgnoreFormatId);
      }

      if (IgnoreCompileCommand.Instance == null)
      {
        await IgnoreCompileCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kIgnoreCompileId);
      }

      if (StopCommand.Instance == null)
      {
        await StopCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kStopClang);
      }

      if (SettingsCommand.Instance == null)
      {
        await SettingsCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kSettingsId);
      }
    }

    public async void Execute(object sender, EventArgs e)
    {
      var freeTrialController = new FreeTrialController();

      // First app install - choose license
      if (freeTrialController.WasEverInTrial() == false && activeAccount == false)
      {
        LicenseView licenseView = new LicenseView();
        licenseView.ShowDialog();
        return;
      }

      // Trial expired
      if (freeTrialController.IsActive() == false && activeAccount == false && tokenExists == false)
      {
        TrialExpiredView trialExpiredView = new TrialExpiredView();
        trialExpiredView.ShowDialog();
        return;
      }

      // Session Expired
      if (freeTrialController.IsActive() == false && activeAccount == false && tokenExists == true)
      {
        LoginView loginView = new LoginView();
        loginView.ShowDialog();
        return;
      }

      var command = CreateCommand(sender);
      if (command == null)
        return;

      await LaunchCommandAsync(command.CommandID.ID, commandUILocation);
    }

    public async Task LaunchCommandAsync(int aCommandId, CommandUILocation aCommandUILocation)
    {
      switch (aCommandId)
      {
        case CommandIds.kSettingsId:
          {
            await SettingsCommand.Instance.ShowSettingsAsync();
            break;
          }
        case CommandIds.kStopClang:
          {
            await StopCommand.Instance.RunStopClangCommandAsync(false);
            break;
          }
        case CommandIds.kClangFormat:
          {
            clearOutputOnFormat = true;
            FormatCommand.Instance.RunClangFormat(aCommandUILocation);
            break;
          }
        case CommandIds.kClangFormatToolbarId:
          {
            clearOutputOnFormat = true;
            FormatCommand.Instance.RunClangFormat(aCommandUILocation);
            break;
          }
        case CommandIds.kCompileId:
          {
            OnBeforeClangCommand(CommandIds.kCompileId);
            await StopBackgroundRunnersAsync();

            await CompileCommand.Instance.RunClangCompileAsync(CommandIds.kCompileId, aCommandUILocation);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kCompileToolbarId:
          {
            OnBeforeClangCommand(CommandIds.kCompileId);
            await StopBackgroundRunnersAsync();

            await CompileCommand.Instance.RunClangCompileAsync(CommandIds.kCompileId, aCommandUILocation);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kTidyId:
          {
            OnBeforeClangCommand(CommandIds.kTidyId);
            await StopBackgroundRunnersAsync();

            await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyId, aCommandUILocation);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kTidyToolbarId:
          {
            OnBeforeClangCommand(CommandIds.kTidyId);
            await StopBackgroundRunnersAsync();

            await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyId, aCommandUILocation);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kTidyFixId:
          {
            OnBeforeClangCommand(CommandIds.kTidyFixId);
            await StopBackgroundRunnersAsync();

            await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyFixId, aCommandUILocation);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kTidyFixToolbarId:
          {
            OnBeforeClangCommand(CommandIds.kTidyFixId);
            await StopBackgroundRunnersAsync();

            await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyFixId, aCommandUILocation);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kIgnoreFormatId:
          {
            IgnoreFormatCommand.Instance.RunIgnoreFormatCommand();
            break;

          }
        case CommandIds.kIgnoreCompileId:
          {
            IgnoreCompileCommand.Instance.RunIgnoreCompileCommand();
            break;
          }
        default:
          break;
      }
    }

    #endregion

    #region Private Methods

    private OleMenuCommand CreateCommand(object sender)
    {
      OleMenuCommand command = null;
      if ((sender is OleMenuCommand) == false)
        return null;

      command = sender as OleMenuCommand;
      if (running && command.CommandID.ID != CommandIds.kStopClang)
        return null;

      if (command.CommandID.ID != CommandIds.kStopClang)
        currentCommand = command.CommandID.ID;

      SetCommandLocation();
      return command;
    }

    private void SetCommandLocation()
    {
      switch (currentCommand)
      {
        case CommandIds.kClangFormatToolbarId:
        case CommandIds.kCompileToolbarId:
        case CommandIds.kTidyToolbarId:
        case CommandIds.kTidyFixToolbarId:
          commandUILocation = CommandUILocation.Toolbar;
          break;
        default:
          commandUILocation = CommandUILocation.ContextMenu;
          break;
      }
    }

    private async Task StopBackgroundRunnersAsync()
    {
      await StopCommand.Instance.RunStopClangCommandAsync(true);
      BackgroundTidyCommand.Running = false;
    }

    private void OnBeforeClangCommand(int aCommandId)
    {
      currentCommand = aCommandId;
      running = true;

      if (OutputWindowConstants.commandName.ContainsKey(aCommandId))
      {
        DisplayStartedMessage(aCommandId, true);
      }

      OnClangCommandBegin(new ClearEventArgs());
    }


    private void OnClangCommandBegin(ClearEventArgs e)
    {
      ClearErrorListEvent?.Invoke(this, e);
    }

    private void OnAfterClangCommand()
    {
      running = false;
      var cMakeBuilder = new CMakeBuilder();
      cMakeBuilder.ClearBuildCashe();
    }

    public void OnAfterFormatCommand(object sender, FormatCommandEventArgs e)
    {
      currentCommand = CommandIds.kClangFormat;
      //if (e.CanFormat)
      //{
      //  ClearOutputWindowEvent?.Invoke(this, new ClearEventArgs());
      //  StatusBarHandler.Status(OutputWindowConstants.commandName[currentCommand] + " finished", 0, vsStatusAnimation.vsStatusAnimationBuild, 0);
      //}
      //else if (e.IgnoreExtension)
      //{
      //  DisplayCannotFormatMessage(e.Clear, 
      //    $"\n--- WARNING ---\nCannot use clang-format on unspecified file types.\nTo enable clang-format add the \"{Path.GetExtension(e.FileName)}\" extension to Clang Power Tools settings -> Format -> File extensions");
      //}
      //else if (e.IgnoreFile)
      //{
      //  DisplayCannotFormatMessage(e.Clear, 
      //    $"\n--- WARNING ---\nCannot use clang-format on ignored files.\nTo enable clang-format remove \"{e.FileName}\" from Clang Power Tools settings -> Format -> Files to ignore.");
      //}
      //else
      //{
      //  DisplayCannotFormatMessage(e.Clear,
      //    $"\n--- ERROR ---\nFormat config file not found.\nCreate the config file and place it in the solution folder or select one of the predefined format styles from Clang Power Tools settings -> Format -> Style.");
      //}
    }

    public void OnAfterRunCommand(object sender, CloseDataStreamingEventArgs e)
    {
      if (BackgroundTidyCommand.Running)
      {
        OnErrorDetected(new EventArgs());
        return;
      }

      if (e.IsStopped)
      {
        DisplayStoppedMessage(false);
        return;
      }

      if (commandUILocation == CommandUILocation.ContextMenu)
      {
        DisplayFinishedMessage(false);
      }
      else if (commandUILocation == CommandUILocation.Toolbar && isActiveDocument)
      {
        DisplayFinishedMessage(false);
      }

      OnErrorDetected(new EventArgs());
    }

    protected void OnErrorDetected(EventArgs e)
    {
      ErrorDetectedEvent?.Invoke(this, e);

      if (BackgroundTidyCommand.Running == false)
        HasEncodingErrorEvent.Invoke(this, new EventArgs());
    }

    public void OnEncodingErrorDetected(object sender, HasEncodingErrorEventArgs e)
    {
      if (!e.Model.HasEncodingError)
      {
        return;
      }
      DisplayErrorWindow();
    }

    private void DisplayErrorWindow()
    {
      try
      {
        UIUpdater.InvokeAsync(new Action(() =>
        {
          var window = new EncodingErrorView(ItemsCollector.GetDocumentsToEncode());
          window.ShowDialog();
        })).SafeFireAndForget();
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    public void OnActiveDocumentCheck(object sender, ActiveDocumentEventArgs e)
    {
      if (e.IsActiveDocument == false)
      {
        DisplayNoActiveDocumentMessage(true);
      }
      isActiveDocument = e.IsActiveDocument;
    }

    private void OnClangCommandMessageTransfer(ClangCommandMessageEventArgs e)
    {
      ClangCommandMessageEvent?.Invoke(this, e);
    }

    public void OnFileHierarchyChanged(object sender, VsHierarchyDetectedEventArgs e)
    {
      HierarchyDetectedEvent?.Invoke(this, e);
    }

    private void DisplayStartedMessage(int aCommandId, bool clearOutput)
    {
      OnClangCommandMessageTransfer(new ClangCommandMessageEventArgs($"\n--- {OutputWindowConstants.commandName[aCommandId].ToUpper()} STARTED ---\n", clearOutput));
      StatusBarHandler.Status(OutputWindowConstants.commandName[aCommandId] + " started...", 1, vsStatusAnimation.vsStatusAnimationBuild, 1);
    }

    private void DisplayNoActiveDocumentMessage(bool clearOutput)
    {
      OnClangCommandMessageTransfer(new ClangCommandMessageEventArgs($"\nToolbar Clang commands can only run on open files. Open a file or use the context menu commands by right-clicking in the Solution Explorer.\n", clearOutput));
      StatusBarHandler.Status("Ready", 0, vsStatusAnimation.vsStatusAnimationBuild, 0);
    }

    private void DisplayFinishedMessage(bool clearOutput)
    {
      OnClangCommandMessageTransfer(new ClangCommandMessageEventArgs($"\n--- {OutputWindowConstants.commandName[currentCommand].ToUpper()} FINISHED ---\n", clearOutput));
      StatusBarHandler.Status(OutputWindowConstants.commandName[currentCommand] + " finished", 0, vsStatusAnimation.vsStatusAnimationBuild, 0);
    }

    private void DisplayStoppedMessage(bool clearOutput)
    {
      OnClangCommandMessageTransfer(new ClangCommandMessageEventArgs($"\n--- {OutputWindowConstants.commandName[currentCommand].ToUpper()} STOPPED ---", clearOutput));
      StatusBarHandler.Status("Command stopped", 0, vsStatusAnimation.vsStatusAnimationBuild, 0);
    }

    private void DisplayCannotFormatMessage(bool clearOutput, string message)
    {
      OnClangCommandMessageTransfer(new ClangCommandMessageEventArgs(message, clearOutput));
      StatusBarHandler.Status("Command stopped", 0, vsStatusAnimation.vsStatusAnimationBuild, 0);
    }


    private string GetCommandName(string aGuid, int aId)
    {
      try
      {
        if (null == aGuid)
          return string.Empty;

        if (null == mCommand)
          return string.Empty;

        Command cmd = mCommand.Item(aGuid, aId);
        if (null == cmd)
          return string.Empty;

        return cmd.Name;
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }

      return string.Empty;
    }

    #endregion


    #region Events


    public void OnLicenseChanged(object sender, LicenseEventArgs e)
    {
      activeAccount = e.IsLicenseActive;
      tokenExists = e.TokenExists;
    }

    internal void OnItemIgnore(object sender, ClangCommandMessageEventArgs e)
    {
      ClangCommandMessageEvent?.Invoke(this, e);
    }

    public void VisibilityOnBeforeCommand(object sender, EventArgs e)
    {
      if (sender is OleMenuCommand == false)
        return;
      var command = (OleMenuCommand)sender;

      var itemsCollector = new ItemsCollector();
      itemsCollector.CollectSelectedProjectItems();
      command.Enabled = itemsCollector.HaveItems;
    }

    /// <summary>
    /// It is called before every command. Update the running state.  
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void OnBeforeClangCommand(object sender, EventArgs e)
    {
      if (!(sender is OleMenuCommand command))
        return;

      if (IsAToolbarCommand(command))
      {
        if (SolutionInfo.AreToolbarCommandsEnabled() == false)
        {
          command.Enabled = false;
          return;
        }
      }
      else if (SolutionInfo.AreContextMenuCommandsEnabled() == false)
      {
        command.Enabled = false;
        return;
      }


      if (VsServiceProvider.TryGetService(typeof(DTE), out object dte) && !(dte as DTE2).Solution.IsOpen)
      {
        command.Visible = command.Enabled = false;
      }
      else if (vsBuildRunning && command.CommandID.ID != CommandIds.kSettingsId)
      {
        command.Visible = command.Enabled = false;
      }
      else
      {
        command.Visible = command.Enabled = command.CommandID.ID != CommandIds.kStopClang ? !running : running;
      }
    }


    /// <summary>
    /// Set the VS running build flag to true when the VS build begin.
    /// </summary>
    /// <param name="Scope"></param>
    /// <param name="Action"></param>
    public void OnMSVCBuildBegin(vsBuildScope Scope, vsBuildAction Action) => vsBuildRunning = true;


    /// <summary>
    /// Set the VS running build flag to false when the VS build finished.
    /// </summary>
    /// <param name="Scope"></param>
    /// <param name="Action"></param>
    public void OnMSVCBuildDone(vsBuildScope Scope, vsBuildAction Action)
    {
      vsBuildRunning = false;
      OnMSVCBuildSucceededAsync().SafeFireAndForget();
    }

    private async Task OnMSVCBuildSucceededAsync()
    {
      if (!CompileCommand.Instance.VsCompileFlag)
        return;

      var exitCode = int.MaxValue;
      if (VsServiceProvider.TryGetService(typeof(DTE), out object dte))
        exitCode = (dte as DTE2).Solution.SolutionBuild.LastBuildInfo;

      // VS compile detected errors and there is not necessary to run clang compile
      if (0 != exitCode)
      {
        CompileCommand.Instance.VsCompileFlag = false;
        return;
      }

      // Run clang compile after the VS compile succeeded

      OnBeforeClangCommand(CommandIds.kCompileId);
      await CompileCommand.Instance.RunClangCompileAsync(CommandIds.kCompileId, CommandUILocation.ContextMenu);
      CompileCommand.Instance.VsCompileFlag = false;
      OnAfterClangCommand();
    }

    public void OnBeforeSave(object sender, Document aDocument)
    {
      BeforeSaveClangTidy(aDocument);
      BeforeSaveClangFormat(aDocument);
    }

    private void BeforeSaveClangTidy(Document document)
    {
      if (false == mSaveCommandWasGiven) // The save event was not triggered by Save File or SaveAll commands
        return;

      TidySettingsModel tidySettings = settingsProvider.GetTidySettingsModel();

      if (false == tidySettings.TidyOnSave) // The clang-tidy on save option is disable 
      {
        OnBeforeActiveDocumentChange(new object(), document);
        return;
      }

      if (true == running) // Clang compile/tidy command is running
        return;

      OnBeforeClangCommand(CommandIds.kTidyFixId);
      TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyFixId, CommandUILocation.ContextMenu).SafeFireAndForget();
      OnAfterClangCommand();

      mSaveCommandWasGiven = false;
    }


    private void BeforeSaveClangFormat(Document aDocument)
    {
      FormatSettingsModel formatSettings = settingsProvider.GetFormatSettingsModel();
      TidySettingsModel tidySettings = settingsProvider.GetTidySettingsModel();

      if (currentCommand == CommandIds.kTidyFixId && running && tidySettings.FormatAfterTidy && formatSettings.FormatOnSave)
      {
        mFormatAfterTidyFlag = true;
        return;
      }

      if (false == formatSettings.FormatOnSave)
        return;

      if (false == Vsix.IsDocumentDirty(aDocument) && false == mFormatAfterTidyFlag)
        return;

      FormatCommand.Instance.FormatOnSave(aDocument);
    }

    public void CommandEventsBeforeExecute(string aGuid, int aId, object aCustomIn, object aCustomOut, ref bool aCancelDefault)
    {
      BeforeExecuteClangCompile(aGuid, aId);
      BeforeExecuteClangTidy(aGuid, aId);
    }


    private void BeforeExecuteClangCompile(string aGuid, int aId)
    {
      CompilerSettingsModel compilerSettings = settingsProvider.GetCompilerSettingsModel();

      if (compilerSettings.ClangAfterMSVC == false)
        return;

      string commandName = GetCommandName(aGuid, aId);
      if (0 != string.Compare("Build.Compile", commandName))
        return;

      CompileCommand.Instance.VsCompileFlag = true;
    }


    private void BeforeExecuteClangTidy(string aGuid, int aId)
    {
      string commandName = GetCommandName(aGuid, aId);
      if (0 != string.Compare("File.SaveAll", commandName) &&
        0 != string.Compare("File.SaveSelectedItems", commandName))
      {
        return;
      }
      mSaveCommandWasGiven = true;
    }

    public void OnBeforeActiveDocumentChange(object sender, Document document)
    {
      CompilerSettingsModel compilerSettings = settingsProvider.GetCompilerSettingsModel();
      if (compilerSettings.ShowSquiggles == false)
        return;

      if (running || vsBuildRunning)
        return;

      _ = Task.Run(async () =>
        {
          await semaphoreSlim.WaitAsync();
          try
          {
            TaskErrorViewModel.Errors.Clear();
            TaskErrorViewModel.FileErrorsPair.Clear();

            var backgroundTidyCommand = new BackgroundTidyCommand(document);
            await backgroundTidyCommand.RunClangTidyAsync();
          }
          finally
          {
            semaphoreSlim.Release();
          }
        });
    }

    private bool IsAToolbarCommand(OleMenuCommand command)
    {
      return command.CommandID.ID == CommandIds.kCompileToolbarId || command.CommandID.ID == CommandIds.kClangFormatToolbarId ||
        command.CommandID.ID == CommandIds.kTidyToolbarId || command.CommandID.ID == CommandIds.kTidyFixToolbarId;
    }

    #endregion
  }
}
