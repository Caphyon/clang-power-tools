using ClangPowerTools.Commands;
using ClangPowerTools.Events;
using ClangPowerTools.Handlers;
using ClangPowerTools.Services;
using ClangPowerTools.Views;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools
{
  /// <summary>
  /// Contains all the logic of disable and enable the clang custom commands  
  /// </summary>
  public class CommandController
  {
    #region Members

    public static readonly Guid mCommandSet = new Guid("498fdff5-5217-4da9-88d2-edad44ba3874");
    private Commands2 mCommand;
    private bool mSaveCommandWasGiven = false;
    private CommandUILocation commandUILocation;
    private int currentCommand;
    private bool mFormatAfterTidyFlag = false;
    private bool isActiveDocument = true;
    public bool running = false;
    public bool vsBuildRunning = false;
    public bool activeLicense = false;

    public event EventHandler<VsHierarchyDetectedEventArgs> HierarchyDetectedEvent;
    public event EventHandler<ClangCommandMessageEventArgs> ClangCommandMessageEvent;
    public event EventHandler<MissingLlvmEventArgs> MissingLlvmEvent;
    public event EventHandler<ClearErrorListEventArgs> ClearErrorListEvent;

    #endregion

    #region Constructor

    public CommandController(AsyncPackage aAsyncPackage)
    {
      if (VsServiceProvider.TryGetService(typeof(DTE), out object dte))
      {
        var dte2 = dte as DTE2;
        mCommand = dte2.Commands as Commands2;
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

      if (TidyConfigCommand.Instance == null)
      {
        await TidyConfigCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kITidyExportConfigId);
      }
    }

    public async void Execute(object sender, EventArgs e)
    {
      if(activeLicense == false)
      {
        LoginView loginView = new LoginView();
        loginView.ShowDialog();
        return;
      }

      var command = CreateCommand(sender);

      if (command == null)
      {
        return;
      }

      await LaunchCommandAsync(command.CommandID.ID, commandUILocation);
    }

    public async Task LaunchCommandAsync(int aCommandId, CommandUILocation aCommandUILocation)
    {
      switch (aCommandId)
      {
        case CommandIds.kSettingsId:
          {
            SettingsCommand.Instance.ShowSettings();
            break;
          }
        case CommandIds.kStopClang:
          {
            await StopCommand.Instance.RunStopClangCommandAsync();
            break;
          }
        case CommandIds.kClangFormat:
          {
            FormatCommand.Instance.RunClangFormat(aCommandUILocation);
            OnAfterFormatCommand();
            break;
          }
        case CommandIds.kClangFormatToolbarId:
          {
            FormatCommand.Instance.RunClangFormat(aCommandUILocation);
            OnAfterFormatCommand();
            break;
          }
        case CommandIds.kCompileId:
          {
            OnBeforeClangCommand(CommandIds.kCompileId);
            await CompileCommand.Instance.RunClangCompileAsync(CommandIds.kCompileId, aCommandUILocation);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kCompileToolbarId:
          {
            OnBeforeClangCommand(CommandIds.kCompileId);
            await CompileCommand.Instance.RunClangCompileAsync(CommandIds.kCompileId, aCommandUILocation);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kTidyId:
          {
            OnBeforeClangCommand(CommandIds.kTidyId);
            await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyId, aCommandUILocation);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kTidyToolbarId:
          {
            OnBeforeClangCommand(CommandIds.kTidyId);
            await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyId, aCommandUILocation);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kTidyFixId:
          {
            OnBeforeClangCommand(CommandIds.kTidyFixId);
            await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyFixId, aCommandUILocation);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kTidyFixToolbarId:
          {
            OnBeforeClangCommand(CommandIds.kTidyFixId);
            await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyFixId, aCommandUILocation);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kITidyExportConfigId:
          {
            TidyConfigCommand.Instance.ExportConfig();
            break;
          }
        case CommandIds.kIgnoreFormatId:
          {
            IgnoreFormatCommand.Instance.RunIgnoreFormatCommand(CommandIds.kIgnoreFormatId);
            break;
          }
        case CommandIds.kIgnoreCompileId:
          {
            IgnoreCompileCommand.Instance.RunIgnoreCompileCommand(CommandIds.kIgnoreCompileId);
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
      {
        return null;
      }

      command = sender as OleMenuCommand;

      if (running && command.CommandID.ID != CommandIds.kStopClang)
      {
        return null;
      }

      if (command.CommandID.ID != CommandIds.kStopClang)
      {
        currentCommand = command.CommandID.ID;
      }
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

    private void OnBeforeClangCommand(int aCommandId)
    {
      currentCommand = aCommandId;
      running = true;

      if (OutputWindowConstants.commandName.ContainsKey(aCommandId))
      {
        DisplayStartedMessage(aCommandId, true);
      }

      OnClangCommandBegin(new ClearErrorListEventArgs());
    }


    private void OnClangCommandBegin(ClearErrorListEventArgs e)
    {
      ClearErrorListEvent?.Invoke(this, e);
    }

    private void OnAfterClangCommand()
    {
      running = false;
    }

    private void OnAfterFormatCommand()
    {
      if (isActiveDocument)
      {
        DisplayFinishedMessage(true);
      }
    }

    public void OnAfterRunCommand(object sender, CloseDataStreamingEventArgs e)
    {
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


    public void OnMissingLLVMDetected(object sender, MissingLlvmEventArgs e)
    {
      MissingLlvmEvent?.Invoke(this, e);
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
      catch (Exception) { }

      return string.Empty;
    }

    #endregion


    #region Events


    public void OnLicenseChanged(object sender, ActiveDocumentEventArgs e)
    {
      activeLicense = e.IsActiveDocument;
    }

    /// <summary>
    /// It is called before every command. Update the running state.  
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void OnBeforeClangCommand(object sender, EventArgs e)
    {
      UIUpdater.Invoke(() =>
      {
        if (!(sender is OleMenuCommand command))
          return;

        if (VsServiceProvider.TryGetService(typeof(DTE), out object dte) && !(dte as DTE2).Solution.IsOpen)
          command.Visible = command.Enabled = false;

        else if (vsBuildRunning && command.CommandID.ID != CommandIds.kSettingsId)
          command.Visible = command.Enabled = false;

        else
          command.Visible = command.Enabled = command.CommandID.ID != CommandIds.kStopClang ? !running : running;
      });
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
    public async void OnMSVCBuildDone(vsBuildScope Scope, vsBuildAction Action)
    {
      vsBuildRunning = false;
      await OnMSVCBuildSucceededAsync();
    }


    private async System.Threading.Tasks.Task OnMSVCBuildSucceededAsync()
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
      BeforeSaveClangTidy();
      BeforeSaveClangFormat(aDocument);
    }


    private void BeforeSaveClangTidy()
    {
      if (false == mSaveCommandWasGiven) // The save event was not triggered by Save File or SaveAll commands
        return;

      var tidyOption = SettingsProvider.TidySettings;

      if (false == tidyOption.AutoTidyOnSave) // The clang-tidy on save option is disable 
        return;

      if (true == running) // Clang compile/tidy command is running
        return;

      TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyFixId, CommandUILocation.ContextMenu);
      mSaveCommandWasGiven = false;
    }


    private void BeforeSaveClangFormat(Document aDocument)
    {
      var clangFormatOptionPage = SettingsProvider.ClangFormatSettings;
      var tidyOptionPage = SettingsProvider.TidySettings;

      if (currentCommand == CommandIds.kTidyFixId && running && tidyOptionPage.FormatAfterTidy && clangFormatOptionPage.EnableFormatOnSave)
      {
        mFormatAfterTidyFlag = true;
        return;
      }

      if (false == clangFormatOptionPage.EnableFormatOnSave)
        return;

      if (false == Vsix.IsDocumentDirty(aDocument) && false == mFormatAfterTidyFlag)
        return;

      if (false == FileHasExtension(aDocument.FullName, clangFormatOptionPage.FileExtensions))
        return;

      if (true == SkipFile(aDocument.FullName, clangFormatOptionPage.FilesToIgnore))
        return;

      FormatCommand.Instance.FormatOnSave(aDocument);
    }


    private bool SkipFile(string aFilePath, string aSkipFiles)
    {
      var skipFilesList = aSkipFiles.ToLower().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
      return skipFilesList.Contains(Path.GetFileName(aFilePath).ToLower());
    }


    private bool FileHasExtension(string filePath, string fileExtensions)
    {
      var extensions = fileExtensions.ToLower().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
      return extensions.Contains(Path.GetExtension(filePath).ToLower());
    }


    public void CommandEventsBeforeExecute(string aGuid, int aId, object aCustomIn, object aCustomOut, ref bool aCancelDefault)
    {
      BeforeExecuteClangCompile(aGuid, aId);
      BeforeExecuteClangTidy(aGuid, aId);
    }


    private void BeforeExecuteClangCompile(string aGuid, int aId)
    {
      var generalOptions = SettingsProvider.GeneralSettings;

      if (null == generalOptions || false == generalOptions.ClangCompileAfterVsCompile)
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

    #endregion
  }
}
