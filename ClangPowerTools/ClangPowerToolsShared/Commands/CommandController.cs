using ClangPowerTools.CMake;
using ClangPowerTools.Commands;
using ClangPowerTools.Commands.BackgroundTidy;
using ClangPowerTools.Error;
using ClangPowerTools.Events;
using ClangPowerTools.Handlers;
using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Views;
using ClangPowerTools.Services;
using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.Helpers;
using ClangPowerToolsShared.MVVM.Views.ToolWindows;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

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
    public bool tokenExists = false;
    public bool clearOutputOnFormat = false;
    public bool keepJsonCompilationDb = false;

    public bool showOpenFolderWarning = true;

    public static readonly Guid mCommandSet = new Guid("498fdff5-5217-4da9-88d2-edad44ba3874");

    public event EventHandler<VsHierarchyDetectedEventArgs> HierarchyDetectedEvent;
    public event EventHandler<ClangCommandMessageEventArgs> ClangCommandMessageEvent;
    public event EventHandler<ClearEventArgs> ClearErrorListEvent;
    public event EventHandler<ClearEventArgs> ClearOutputWindowEvent;
    public event EventHandler<EventArgs> ErrorDetectedEvent;
    public event EventHandler<EventArgs> HasEncodingErrorEvent;

    public RunningDocTableEvents mRunningDocTableEvents;

    private bool mFormatted = false;
    private readonly Commands2 mCommand;
    private CommandUILocation commandUILocation;
    private int currentCommand = 0;
    private bool mSaveCommandWasGiven = false;
    private bool mFormatAfterTidyFlag = false;
    private string oldActiveDocumentName = null;
    private AsyncPackage package;
    private LaunchCompilationDbProgrammatically launchCompilationDbProgrammatically = new();
    public Dictionary<string, bool> SavedDocuments = new();


    public AsyncPackage Package
    {
      get { return package; }
    }

    private readonly object mutex = new object();

    private readonly string registryName = @"Software\Caphyon\Clang Power Tools";
    private readonly string keyName = "CMakeBetaWarning";

    #endregion

    #region Constructor

    public CommandController(AsyncPackage aAsyncPackage)
    {
      SavedDocuments = new Dictionary<string, bool>();
      package = aAsyncPackage;
      if (VsServiceProvider.TryGetService(typeof(DTE2), out object dte))
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
        await CompileCommand.InitializeAsync(this, aAsyncPackage, mCommandSet,
                                             CommandIds.kCompileId);
        await CompileCommand.InitializeAsync(this, aAsyncPackage, mCommandSet,
                                             CommandIds.kCompileToolbarId);
      }

      if (TidyCommand.Instance == null)
      {
        await TidyCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kTidyId);
        await TidyCommand.InitializeAsync(this, aAsyncPackage, mCommandSet,
                                          CommandIds.kTidyToolbarId);
      }

      if (FormatCommand.Instance == null)
      {
        await FormatCommand.InitializeAsync(this, aAsyncPackage, mCommandSet,
                                            CommandIds.kClangFormat);
        await FormatCommand.InitializeAsync(this, aAsyncPackage, mCommandSet,
                                            CommandIds.kClangFormatToolbarId);
      }

      if (FindCommand.Instance == null)
      {
        await FindCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kClangFind);
      }

      if (IgnoreFormatCommand.Instance == null)
      {
        await IgnoreFormatCommand.InitializeAsync(this, aAsyncPackage, mCommandSet,
                                                  CommandIds.kIgnoreFormatId);
      }

      if (IgnoreCompileCommand.Instance == null)
      {
        await IgnoreCompileCommand.InitializeAsync(this, aAsyncPackage, mCommandSet,
                                                   CommandIds.kIgnoreCompileId);
      }

      if (StopCommand.Instance == null)
      {
        await StopCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kStopClang);
      }

      if (JsonCompilationDatabaseCommand.Instance == null)
      {
        await JsonCompilationDatabaseCommand.InitializeAsync(this, aAsyncPackage, mCommandSet,
                                                             CommandIds.kJsonCompilationDatabase);
      }

      if (DocumentationYamlCommand.Instance == null)
      {
        await DocumentationYamlCommand.InitializeAsync(this, aAsyncPackage, mCommandSet,
                                                       CommandIds.kDocumentationYamlId);
      }

      if (DocumentationHtmlCommand.Instance == null)
      {
        await DocumentationHtmlCommand.InitializeAsync(this, aAsyncPackage, mCommandSet,
                                                       CommandIds.kDocumentationHtmlId);
      }

      if (DocumentationMdCommand.Instance == null)
      {
        await DocumentationMdCommand.InitializeAsync(this, aAsyncPackage, mCommandSet,
                                                     CommandIds.kDocumentationMdId);
      }

      if (SettingsCommand.Instance == null)
      {
        await SettingsCommand.InitializeAsync(this, aAsyncPackage, mCommandSet,
                                              CommandIds.kSettingsId);
      }

      if (FindViewMenuCommand.Instance == null)
      {
        await FindViewMenuCommand.InitializeAsync(this, aAsyncPackage, mCommandSet,
                                                  CommandIds.kFindViewMenuId);
      }
    }

    public async void Execute(object sender, EventArgs e)
    {
      // var freeTrialController = new FreeTrialController();

      // First app install - choose license
      // if (SettingsProvider.AccountModel.LicenseType == LicenseType.NoLicense)
      //{
      //  LicenseView licenseView = new LicenseView();
      //  licenseView.ShowDialog();
      //  return;
      //}

      // Trial expired
      // if (SettingsProvider.AccountModel.LicenseType == LicenseType.Trial &&
      // freeTrialController.IsActive() == false)
      //{
      //  TrialExpiredView trialExpiredView = new TrialExpiredView();
      //  trialExpiredView.ShowDialog();
      //  return;
      //}

      // Session Expired
      // if (SettingsProvider.AccountModel.LicenseType == LicenseType.SessionExpired)
      //{
      //  LoginView loginView = new LoginView();
      //  loginView.ShowDialog();
      //  return;
      //}

      var command = CreateCommand(sender);
      if (command == null)
        return;

      await LaunchCommandAsync(command.CommandID.ID, commandUILocation);
    }

    public int GetCurrentCommandId() { return currentCommand; }

    public async Task LaunchCommandAsync(int aCommandId,
                                         CommandUILocation aCommandUILocation,
                                         List<string> paths = null,
                                         bool openCompilationDatabaseInExplorer = true)
    {
      // If ApplyTidy-Fix is enabled, switch command
      ChooseCommandIdDependingOnTidy(ref aCommandId);

      switch (aCommandId)
      {
        case CommandIds.kFindViewMenuId:
          {
            await StopBackgroundRunnersAsync();
            OnBeforeClangCommand(CommandIds.kFindViewMenuId);

            await FindViewMenuCommand.Instance.FindAsync(aCommandUILocation);
            OnAfterClangCommand();
            break;
          }
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
        case CommandIds.kClangFind:
          {
            await StopBackgroundRunnersAsync();
            OnBeforeClangCommand(CommandIds.kClangFind);

            await FindCommand.Instance.FindAsync(aCommandUILocation);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kClangFindRun:
          {
            await launchCompilationDbProgrammatically.FromFindToolWindowAsync();
            OnBeforeClangCommand(CommandIds.kClangFindRun);

            await FindCommand.Instance.RunQueryAsync();

            OnAfterClangCommand();
            break;
          }
        case CommandIds.kCompileId:
          {
            await StopBackgroundRunnersAsync();
            OnBeforeClangCommand(CommandIds.kCompileId);

            await CompileCommand.Instance.RunClangCompileAsync(CommandIds.kCompileId,
                                                               aCommandUILocation);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kCompileToolbarId:
          {
            await StopBackgroundRunnersAsync();
            OnBeforeClangCommand(CommandIds.kCompileId);

            await CompileCommand.Instance.RunClangCompileAsync(CommandIds.kCompileId,
                                                               aCommandUILocation);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kTidyId:
          {
            await StopBackgroundRunnersAsync();
            OnBeforeClangCommand(CommandIds.kTidyId);

            await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyId, aCommandUILocation);
            await TidyCommand.Instance.ShowTidyToolWindowEmptyAsync();

            OnAfterClangCommand();
            break;
          }
        case CommandIds.kTidyToolbarId:
          {
            await StopBackgroundRunnersAsync();
            OnBeforeClangCommand(CommandIds.kTidyId);

            await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyId, aCommandUILocation);
            await TidyCommand.Instance.ShowTidyToolWindowEmptyAsync();

            OnAfterClangCommand();
            break;
          }
        case CommandIds.kTidyToolWindowId:
          {
            await StopBackgroundRunnersAsync();
            OnBeforeClangCommand(CommandIds.kTidyToolWindowId);

            await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyToolWindowId,
                                                         aCommandUILocation, paths);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kTidyToolWindowFilesId:
          {
            OnAfterClangCommand();
            await StopBackgroundRunnersAsync();

            await TidyCommand.Instance.ShowTidyToolWindowAsync(paths);

            OnAfterClangCommand();
            break;
          }
        case CommandIds.kTidyFixId:
          {
            await StopBackgroundRunnersAsync();
            OnBeforeClangCommand(CommandIds.kTidyFixId);

            await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyFixId, aCommandUILocation,
                                                         paths);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kTidyFixToolbarId:
          {
            await StopBackgroundRunnersAsync();
            OnBeforeClangCommand(CommandIds.kTidyFixId);

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
        case CommandIds.kDocumentationYamlId:
          {
            await launchCompilationDbProgrammatically.FromGenerateDocumentationAsync();

            await StopBackgroundRunnersAsync();
            OnBeforeClangCommand(CommandIds.kDocumentationYamlId);

            await DocumentationYamlCommand.Instance.GenerateDocumentationAsync(
                CommandIds.kDocumentationYamlId);

            OnAfterClangCommand();
            break;
          }
        case CommandIds.kDocumentationMdId:
          {
            await launchCompilationDbProgrammatically.FromGenerateDocumentationAsync();

            await StopBackgroundRunnersAsync();
            OnBeforeClangCommand(CommandIds.kDocumentationMdId);

            await DocumentationMdCommand.Instance.GenerateDocumentationAsync(
                CommandIds.kDocumentationMdId);

            OnAfterClangCommand();
            break;
          }
        case CommandIds.kDocumentationHtmlId:
          {
            await launchCompilationDbProgrammatically.FromGenerateDocumentationAsync();

            await StopBackgroundRunnersAsync();
            OnBeforeClangCommand(CommandIds.kDocumentationHtmlId);

            await DocumentationHtmlCommand.Instance.GenerateDocumentationAsync(
                CommandIds.kDocumentationHtmlId);

            OnAfterClangCommand();
            break;
          }
        case CommandIds.kJsonCompilationDatabase:
          {
            await StopBackgroundRunnersAsync();
            OnBeforeClangCommand(CommandIds.kJsonCompilationDatabase);

            await JsonCompilationDatabaseCommand.Instance.ExportAsync(
                aCommandUILocation, openCompilationDatabaseInExplorer);
            OnAfterClangCommand();
            break;
          }
        default:
          break;
      }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Return commandId depending on tidy settings
    /// </summary>
    /// <param name="commandId"></param>
    /// <returns></returns>
    private void ChooseCommandIdDependingOnTidy(ref int aCommandId)
    {
      var tidySettings = SettingsProvider.TidySettingsModel;
      if (aCommandId == CommandIds.kTidyToolbarId && tidySettings.ApplyTidyFix)
      {
        aCommandId = CommandIds.kTidyFixToolbarId;
      }

      if (aCommandId == CommandIds.kTidyId && tidySettings.ApplyTidyFix)
      {
        aCommandId = CommandIds.kTidyFixId;
      }
    }

    private OleMenuCommand CreateCommand(object sender)
    {
      if ((sender is OleMenuCommand) == false)
        return null;

      OleMenuCommand command = sender as OleMenuCommand;
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
        case CommandIds.kFindViewMenuId:
          commandUILocation = CommandUILocation.ViewMenu;
          break;
        default:
          commandUILocation = CommandUILocation.ContextMenu;
          break;
      }
    }

    private async Task StopBackgroundRunnersAsync()
    {
      await StopCommand.Instance.RunStopClangCommandAsync(true);
    }

    private void StopBackgroundRunners() { StopCommand.Instance.StopClangCommand(true); }

    private void OnBeforeClangCommand(int aCommandId)
    {
      currentCommand = aCommandId;
      running = true;

      OnClangCommandBegin(new ClearEventArgs());

      if (OutputWindowConstants.commandName.ContainsKey(aCommandId))
        DisplayStartedMessage(aCommandId, true);
    }

    private void OnClangCommandBegin(ClearEventArgs e) { ClearErrorListEvent?.Invoke(this, e); }

    private void OnAfterClangCommand()
    {
      running = false;
      var cMakeBuilder = new CMakeBuilder();
      cMakeBuilder.ClearBuildCashe();
    }



    public void OnAfterFormatCommand(object sender, FormatCommandEventArgs e)
    {
      if (e.FormatConfigFound == false)
      {
        DisplayMessage(
            e.Clear,
            $"\n--- ERROR ---\nFormat config file not found.\nCreate the config file and place it in the solution folder or select one of the predefined format styles from Clang Power Tools settings -> Format -> Style.");
      }
      else if (e.Clear)
      {
        ClearOutputWindowEvent?.Invoke(this, new ClearEventArgs());
      }
    }

    public void OnAfterRunCommand(object sender, CloseDataStreamingEventArgs e)
    {
      if (e.IsStopped)
      {
        DisplayStoppedMessage(false);
        running = false;
        return;
      }

      DisplayFinishedMessage(false);

      if (currentCommand != CommandIds.kJsonCompilationDatabase)
        OnErrorDetected(new EventArgs());
    }

    protected void OnErrorDetected(EventArgs e)
    {
      ErrorDetectedEvent?.Invoke(this, e);
      HasEncodingErrorEvent?.Invoke(this, new EventArgs());
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
        UIUpdater
            .InvokeAsync(new Action(() =>
            {
              var itemsCollector = new ItemsCollector();
              var window = new EncodingErrorView(itemsCollector.GetDocumentsToEncode());
              window.ShowDialog();
            }))
            .SafeFireAndForget();
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
      OnClangCommandMessageTransfer(new ClangCommandMessageEventArgs(
          $"\n--- {OutputWindowConstants.commandName[aCommandId].ToUpper()} STARTED ---\n",
          clearOutput));
      StatusBarHandler.Status(OutputWindowConstants.commandName[aCommandId] + " started...", 1,
                              vsStatusAnimation.vsStatusAnimationBuild, 1);
    }

    private void DisplayNoActiveDocumentMessage(bool clearOutput)
    {
      OnClangCommandMessageTransfer(new ClangCommandMessageEventArgs(
          $"\nToolbar Clang commands can only run on open files. Open a file or use the context menu commands by right-clicking in the Solution Explorer.\n",
          clearOutput));
      StatusBarHandler.Status("Ready", 0, vsStatusAnimation.vsStatusAnimationBuild, 0);
    }

    private void DisplayFinishedMessage(bool clearOutput)
    {
      OnClangCommandMessageTransfer(new ClangCommandMessageEventArgs(
          $"\n--- {OutputWindowConstants.commandName[currentCommand].ToUpper()} FINISHED ---\n",
          clearOutput));
      StatusBarHandler.Status(OutputWindowConstants.commandName[currentCommand] + " finished", 0,
                              vsStatusAnimation.vsStatusAnimationBuild, 0);
    }

    private void DisplayStoppedMessage(bool clearOutput)
    {
      OnClangCommandMessageTransfer(new ClangCommandMessageEventArgs(
          $"\n--- {OutputWindowConstants.commandName[currentCommand].ToUpper()} STOPPED ---",
          clearOutput));
      StatusBarHandler.Status("Command stopped", 0, vsStatusAnimation.vsStatusAnimationBuild, 0);
    }

    public void DisplayMessage(bool clearOutput, string message)
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
      catch (Exception)
      {
        // MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }

      return string.Empty;
    }

    #endregion

    #region Events

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
      if (itemsCollector.Items != null && itemsCollector.Items.Count == 1 &&
          (command.CommandID.ID == CommandIds.kIgnoreCompileId) &&
          ScriptConstants.kAcceptedFileExtensionsWithoutHeaders.Contains(
              Path.GetExtension(itemsCollector.Items[0].GetName())) == false)
      {
        command.Visible = command.Enabled = false;
      }
      else
      {
        command.Enabled = !itemsCollector.IsEmpty;
      }
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

      var itemsCollector = new ItemsCollector();
      itemsCollector.CollectSelectedProjectItems();

      var tidySettings = SettingsProvider.TidySettingsModel;
      // Change button name for tidy depending on settings property
      if (tidySettings.ApplyTidyFix && (command.CommandID.ID == CommandIds.kTidyId ||
                                        command.CommandID.ID == CommandIds.kTidyToolbarId))
      {
        command.Text = "Tidy-Fix";
      }
      else if (command.CommandID.ID == CommandIds.kTidyId ||
                 command.CommandID.ID == CommandIds.kTidyToolbarId)
      {
        command.Text = "Tidy";
      }

      if (IsAToolbarCommand(command))
      {
        if (SolutionInfo.AreToolbarCommandsEnabled() == false)
        {
          command.Enabled = command.CommandID.ID == CommandIds.kClangFormatToolbarId &&
                            SolutionInfo.ActiveDocumentValidation();
          return;
        }
      }
      else if (SolutionInfo.AreContextMenuCommandsEnabled() == false)
      {
        command.Enabled = false;
        return;
      }
      if (VsServiceProvider.TryGetService(typeof(DTE2), out object dte) &&
          !(dte as DTE2).Solution.IsOpen)
      {
        command.Visible = command.Enabled = false;
      }
      else if (itemsCollector.Items != null && itemsCollector.Items.Count == 1 &&
                 (command.CommandID.ID == CommandIds.kCompileId ||
                  command.CommandID.ID == CommandIds.kTidyId ||
                  command.CommandID.ID == CommandIds.kJsonCompilationDatabase ||
                  command.CommandID.ID == CommandIds.kClangFindRun ||
                  command.CommandID.ID == CommandIds.kIgnoreCompileId ||
                  command.CommandID.ID == CommandIds.kDocumentationMdId ||
                  command.CommandID.ID == CommandIds.kDocumentationHtmlId ||
                  command.CommandID.ID == CommandIds.kDocumentationYamlId ||
                  command.CommandID.ID == CommandIds.kClangFind) &&
                 ScriptConstants.kAcceptedFileExtensionsWithoutHeaders.Contains(
                     Path.GetExtension(itemsCollector.Items[0].GetName())) == false)
      {
        command.Visible = command.Enabled = false;
        return;
      }
      else if (vsBuildRunning && command.CommandID.ID != CommandIds.kSettingsId)
      {
        command.Visible = command.Enabled = false;
      }
      else
      {
        command.Visible = command.Enabled =
            command.CommandID.ID != CommandIds.kStopClang ? !running : running;
      }
    }

    /// <summary>
    /// Set the VS running build flag to true when the VS build begin.
    /// </summary>
    /// <param name="Scope"></param>
    /// <param name="Action"></param>
    public void OnMSVCBuildBegin(vsBuildScope Scope, vsBuildAction Action)
    {
      vsBuildRunning = true;
    }

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
      if (SettingsProvider.CompilerSettingsModel.ClangAfterMSVC == false ||
          SolutionInfo.ContainsCppProject() == false)
        return;

      var exitCode = int.MaxValue;
      if (VsServiceProvider.TryGetService(typeof(DTE2), out object dte))
        exitCode = (dte as DTE2).Solution.SolutionBuild.LastBuildInfo;

      // VS compile detected errors and there is not necessary to run clang compile
      if (exitCode != 0)
      {
        return;
      }

      // Run clang compile after the VS compile succeeded

      OnBeforeClangCommand(CommandIds.kCompileId);
      await CompileCommand.Instance.RunClangCompileAsync(CommandIds.kCompileId,
                                                         CommandUILocation.ContextMenu);
      OnAfterClangCommand();
    }

    private int HideFindToolWindow()
    {
      var findToolWindow = package.FindToolWindow(typeof(FindToolWindow), 0, false);
      if (findToolWindow is null)
        return VSConstants.S_OK;
      var findWindow = findToolWindow.Frame as IVsWindowFrame;
      findWindow?.Hide();

      return VSConstants.S_OK;
    }

    private int HideTidyToolWindow()
    {
      var tidyToolWindow = package.FindToolWindow(typeof(TidyToolWindow), 0, false);
      if (tidyToolWindow is null)
        return VSConstants.S_OK;
      var tidyWindow = tidyToolWindow.Frame as IVsWindowFrame;
      tidyWindow?.Hide();

      return VSConstants.S_OK;
    }

    public void OnAfterSave(object sender, Document aDocument)
    {
      var formatSettings = SettingsProvider.FormatSettingsModel;

      if (!formatSettings.FormatOnSave)
        return;
      if (SavedDocuments.ContainsKey(aDocument.GetHashCode().ToString()) &&
        SavedDocuments[aDocument.GetHashCode().ToString()])
      {
        SavedDocuments[aDocument.GetHashCode().ToString()] = false;
        return;
      }
      else if (!SavedDocuments.ContainsKey(aDocument.GetHashCode().ToString()))
      {
        SavedDocuments.Add(aDocument.GetHashCode().ToString(), false);
      }

      StopBackgroundRunners();
      BeforeSaveClangTidyAsync(aDocument).SafeFireAndForget();
      BeforeSaveClangFormat(aDocument);
      if (SavedDocuments.ContainsKey(aDocument.GetHashCode().ToString()) &&
        !SavedDocuments[aDocument.GetHashCode().ToString()])
      {
        FormatCommand.Instance.FormatOnSave(aDocument);
        SavedDocuments[aDocument.GetHashCode().ToString()] = true;
      }
      if (mRunningDocTableEvents is not null)
      {
        if (VsServiceProvider.TryGetService(typeof(DTE2), out object dte))
        {
          var dte2 = (DTE2)dte;
          dte2.ExecuteCommand("File.SaveSelectedItems");
        }
      }
    }

    private async Task BeforeSaveClangTidyAsync(Document document)
    {
      // OnBeforeActiveDocumentChange(new object(), document);

      // The save event was not triggered by Save File or SaveAll commands
      if (false == mSaveCommandWasGiven)
        return;

      var tidySettings = SettingsProvider.TidySettingsModel;

      // The clang-tidy on save option is disable
      if (false == tidySettings.TidyOnSave)
        return;

      // Clang compile/tidy command is running
      if (true == running)
        return;

      await StopBackgroundRunnersAsync();
      OnBeforeClangCommand(CommandIds.kTidyFixId);
      await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyFixId,
                                                   CommandUILocation.ContextMenu);
      OnAfterClangCommand();

      mSaveCommandWasGiven = false;
    }

    private void BeforeSaveClangFormat(Document aDocument)
    {
      var formatSettings = SettingsProvider.FormatSettingsModel;
      var tidySettings = SettingsProvider.TidySettingsModel;

      if (currentCommand == CommandIds.kTidyFixId && running && tidySettings.FormatAfterTidy &&
          formatSettings.FormatOnSave)
      {
        mFormatAfterTidyFlag = true;
        return;
      }

      if (false == formatSettings.FormatOnSave)
        return;
    }

    public void CommandEventsBeforeExecute(string aGuid,
                                           int aId,
                                           object aCustomIn,
                                           object aCustomOut,
                                           ref bool aCancelDefault)
    {
      BeforeExecuteClangCompile(aGuid, aId);
      BeforeExecuteClangTidy(aGuid, aId);
    }

    private void BeforeExecuteClangCompile(string aGuid, int aId)
    {
      var compilerSettings = SettingsProvider.CompilerSettingsModel;

      if (compilerSettings.ClangAfterMSVC == false)
        return;

      string commandName = GetCommandName(aGuid, aId);
      if (0 != string.Compare(VsCommands.Compile, commandName))
        return;
    }

    private void BeforeExecuteClangTidy(string aGuid, int aId)
    {
      string commandName = GetCommandName(aGuid, aId);

      if (VsCommands.SaveCommands.Contains(commandName))
        mSaveCommandWasGiven = true;
    }

    public void OnWindowActivated(Window GotFocus, Window LostFocus)
    {
      VsWindowController.SetPreviousActiveWindow(LostFocus);

      if (ReleaseNotesView.WasShown == false)
      {
        var releaseNotesView = new ReleaseNotesView(true);
        releaseNotesView.Show();
      }

      if (showOpenFolderWarning)
      {
        var registryUtility = new RegistryUtility(registryName);
        string showCMakeBetaWarning = registryUtility.ReadCurrentUserKey(keyName);

        if (showCMakeBetaWarning == null && SolutionInfo.IsOpenFolderModeActive())
        {
          showOpenFolderWarning = false;
          CMakeBetaWarning cMakeBetaWarning = new CMakeBetaWarning();
          cMakeBetaWarning.Show();
        }
      }

      if (SettingsProvider.CompilerSettingsModel.ShowSquiggles == false)
        return;

      if (running || vsBuildRunning)
        return;

      Document document = GotFocus.Document;
      if (document == null)
        return;

      if (!string.IsNullOrEmpty(oldActiveDocumentName) &&
          oldActiveDocumentName == document.FullName)
        return;

      oldActiveDocumentName = document.FullName;
      if (!ScriptConstants.kAcceptedFileExtensions.Contains(Path.GetExtension(document.FullName)))
        return;

      _ = Task.Run(() =>
      {
        lock (mutex)
        {
          try
          {
            TaskErrorViewModel.FileErrorsPair.Clear();
            using BackgroundTidy backgroundTidyCommand = new BackgroundTidy();
            backgroundTidyCommand.Run(document, CommandIds.kTidyId);
          }
          catch (Exception e)
          {
            MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          }
        }
      });
    }

    private bool IsAToolbarCommand(OleMenuCommand command)
    {
      return command.CommandID.ID == CommandIds.kCompileToolbarId ||
             command.CommandID.ID == CommandIds.kClangFormatToolbarId ||
             command.CommandID.ID == CommandIds.kTidyToolbarId ||
             command.CommandID.ID == CommandIds.kTidyFixToolbarId;
    }

    #endregion
  }
}
