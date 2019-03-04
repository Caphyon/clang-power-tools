using ClangPowerTools.Commands;
using ClangPowerTools.DialogPages;
using ClangPowerTools.Events;
using ClangPowerTools.Handlers;
using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Linq;

namespace ClangPowerTools
{
  /// <summary>
  /// Contains all the logic of disable and enable the clang custom commands  
  /// </summary>
  public class CommandsController
  {
    #region Members

    public static readonly Guid mCommandSet = new Guid("498fdff5-5217-4da9-88d2-edad44ba3874");
    private AsyncPackage mAsyncPackage;
    private Commands2 mCommand;
    private bool mSaveCommandWasGiven = false;
    private Document mDocument;
    private bool mFormatAfterTidyFlag = false;

    public event EventHandler<VsHierarchyDetectedEventArgs> HierarchyDetectedEvent;

    public event EventHandler<ClangCommandMessageEventArgs> ClangCommandMessageEvent;

    public event EventHandler<MissingLlvmEventArgs> MissingLlvmEvent;

    public event EventHandler<ClearErrorListEventArgs> ClearErrorListEvent;

    #endregion


    #region Properties

    /// <summary>
    /// Store the command id of the current running command
    /// If no command is running then it will have a value less then 0
    /// </summary>
    public int CurrentCommand { get; private set; }

    /// <summary>
    /// Running flag for clang commands
    /// </summary>
    public bool Running { get; set; } = false;


    /// <summary>
    /// Running flag for Visual Studio build
    /// </summary>
    public bool VsBuildRunning { get; set; }

    #endregion


    #region Constructor

    public CommandsController(AsyncPackage aAsyncPackage)
    {
      mAsyncPackage = aAsyncPackage;

      if (VsServiceProvider.TryGetService(typeof(DTE), out object dte))
      {
        var dte2 = dte as DTE2;
        mCommand = dte2.Commands as Commands2;
      }
    }

    #endregion


    #region Public Methods


    public async System.Threading.Tasks.Task InitializeAsyncCommands(AsyncPackage aAsyncPackage)
    {
      if (null == CompileCommand.Instance)
        await CompileCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kCompileId);

      if (null == TidyCommand.Instance)
      {
        await TidyCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kTidyId);
        await TidyCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kTidyFixId);
        await TidyCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kTidyFixMenuId);
      }

      if (null == ClangFormatCommand.Instance)
        await ClangFormatCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kClangFormat);

      if (IgnoreFormatCommand.Instance == null)
        await IgnoreFormatCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kIgnoreFormatId);

      if (IgnoreCompileCommand.Instance == null)
        await IgnoreCompileCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kIgnoreCompileId);

      if (null == StopClang.Instance)
        await StopClang.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kStopClang);

      if (null == SettingsCommand.Instance)
        await SettingsCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kSettingsId);

      if (null == TidyConfigCommand.Instance)
        await TidyConfigCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kITidyExportConfigId);
    }


    public async void ExecuteAsync(object sender, EventArgs e)
    {
      if (!(sender is OleMenuCommand command))
        return;

      if (Running && command.CommandID.ID != CommandIds.kStopClang)
        return;
      
      switch (command.CommandID.ID)
      {
        case CommandIds.kSettingsId:
          {
            CurrentCommand = CommandIds.kSettingsId;
            SettingsCommand.Instance.ShowSettings();
            break;
          }
        case CommandIds.kStopClang:
          {
            CurrentCommand = CommandIds.kStopClang;
            StopClang.Instance.RunStopClangCommand();
            break;
          }
        case CommandIds.kClangFormat:
          {
            CurrentCommand = CommandIds.kClangFormat;
            ClangFormatCommand.Instance.RunClangFormat();
            break;
          }
        case CommandIds.kCompileId:
          {
            OnBeforeClangCommand(CommandIds.kCompileId);
            await CompileCommand.Instance.RunClangCompile(CommandIds.kCompileId);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kTidyId:
          {
            OnBeforeClangCommand(CommandIds.kTidyId);
            await TidyCommand.Instance.RunClangTidy(CommandIds.kTidyId);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kTidyFixId:
        case CommandIds.kTidyFixMenuId:
          {
            OnBeforeClangCommand(CommandIds.kTidyFixId);
            await TidyCommand.Instance.RunClangTidy(CommandIds.kTidyFixId);
            OnAfterClangCommand();
            break;
          }
        case CommandIds.kITidyExportConfigId:
          {
            CurrentCommand = CommandIds.kITidyExportConfigId;
            TidyConfigCommand.Instance.ExportConfig();
            break;
          }

        case CommandIds.kIgnoreFormatId:
          {
            CurrentCommand = CommandIds.kIgnoreFormatId;
            IgnoreFormatCommand.Instance.RunIgnoreFormatCommand(CommandIds.kIgnoreFormatId);
            break;
          }
        case CommandIds.kIgnoreCompileId:
          {
            CurrentCommand = CommandIds.kIgnoreCompileId;
            IgnoreCompileCommand.Instance.RunIgnoreCompileCommand(CommandIds.kIgnoreCompileId);
            break;
          }
        default:
          break;
      }
    }


    #endregion


    #region Private Methods


    private void OnBeforeClangCommand(int aCommandId)
    {
      CurrentCommand = aCommandId;
      Running = true;

      if(OutputWindowConstants.kCommandsNames.ContainsKey(aCommandId))
        OnClangCommandMessageTransfer(new ClangCommandMessageEventArgs($"\nStart {OutputWindowConstants.kCommandsNames[aCommandId]}\n", true));

      OnClangCommandBegin(new ClearErrorListEventArgs());
    }

    private void OnClangCommandBegin(ClearErrorListEventArgs e)
    {
      ClearErrorListEvent?.Invoke(this, e);
    }

    private void OnAfterClangCommand()
    {
      Running = false;
    }

    public void OnCloseCommandDataConnection(object sender, CloseDataConnectionEventArgs e)
    {
      if (OutputWindowConstants.kCommandsNames.ContainsKey(CurrentCommand))
        OnClangCommandMessageTransfer(new ClangCommandMessageEventArgs($"\nDone {OutputWindowConstants.kCommandsNames[CurrentCommand]}\n", false));
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

        else if (VsBuildRunning && command.CommandID.ID != CommandIds.kSettingsId)
          command.Visible = command.Enabled = false;

        else
          command.Visible = command.Enabled = command.CommandID.ID != CommandIds.kStopClang ? !Running : Running;
      });
    }


    /// <summary>
    /// Set the VS running build flag to true when the VS build begin.
    /// </summary>
    /// <param name="Scope"></param>
    /// <param name="Action"></param>
    public void OnMSVCBuildBegin(vsBuildScope Scope, vsBuildAction Action) => VsBuildRunning = true;


    /// <summary>
    /// Set the VS running build flag to false when the VS build finished.
    /// </summary>
    /// <param name="Scope"></param>
    /// <param name="Action"></param>
    public void OnMSVCBuildDone(vsBuildScope Scope, vsBuildAction Action)
    {
      VsBuildRunning = false;
      OnMSVCBuildSucceededAsync();
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
      await CompileCommand.Instance.RunClangCompile(CommandIds.kCompileId);
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

      if (true == Running) // Clang compile/tidy command is running
        return;

      TidyCommand.Instance.RunClangTidy(CommandIds.kTidyFixId);
      mSaveCommandWasGiven = false;
    }


    private void BeforeSaveClangFormat(Document aDocument)
    {
      var clangFormatOptionPage = SettingsProvider.ClangFormatSettings;
      var tidyOptionPage = SettingsProvider.TidySettings;

      if (CurrentCommand == CommandIds.kTidyFixId && Running && tidyOptionPage.FormatAfterTidy && clangFormatOptionPage.EnableFormatOnSave)
      {
        mDocument = aDocument;
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

      var option = SettingsProvider.ClangFormatSettings;
      ClangFormatCommand.Instance.FormatDocument(aDocument, option);
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
