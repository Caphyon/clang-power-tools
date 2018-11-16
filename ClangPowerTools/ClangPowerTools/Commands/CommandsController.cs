using ClangPowerTools.Commands;
using ClangPowerTools.DialogPages;
using ClangPowerTools.Handlers;
using ClangPowerTools.Output;
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

    #endregion


    #region Properties


    /// <summary>
    /// Running flag for clang commands
    /// </summary>
    public bool Running { get; set; }


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


    public void Execute(object sender, EventArgs e)
    {
      if (!(sender is OleMenuCommand command))
        return;

      switch (command.CommandID.ID)
      {
        case CommandIds.kSettingsId:
          SettingsCommand.Instance.ShowSettings();
          break;

        case CommandIds.kStopClang:
          StopClang.Instance.RunStopClangCommand();
          break;

        case CommandIds.kClangFormat:
          ClangFormatCommand.Instance.RunClangFormat();
          break;

        case CommandIds.kCompileId:
          CompileCommand.Instance.RunClangCompile(CommandIds.kCompileId);
          break;

        case CommandIds.kTidyId:
          TidyCommand.Instance.RunClangTidy(CommandIds.kTidyId, false);
          break;

        case CommandIds.kTidyFixId:
          TidyCommand.Instance.RunClangTidy(CommandIds.kTidyFixId, true);
          break;
      }
    }


    public async System.Threading.Tasks.Task InitializeAsyncCommands(AsyncPackage aAsyncPackage,
      ErrorWindowController aErrorController, OutputWindowController aOutputWindowController)
    {
      if (null == CompileCommand.Instance)
        await CompileCommand.InitializeAsync(this, aErrorController, aOutputWindowController, aAsyncPackage, mCommandSet, CommandIds.kCompileId);

      if (null == TidyCommand.Instance)
      {
        await TidyCommand.InitializeAsync(this, aErrorController, aOutputWindowController, aAsyncPackage, mCommandSet, CommandIds.kTidyId);
        await TidyCommand.InitializeAsync(this, aErrorController, aOutputWindowController, aAsyncPackage, mCommandSet, CommandIds.kTidyFixId);
      }

      if (null == ClangFormatCommand.Instance)
        await ClangFormatCommand.InitializeAsync(this, aErrorController, aOutputWindowController, aAsyncPackage, mCommandSet, CommandIds.kClangFormat);

      if (null == StopClang.Instance)
        await StopClang.InitializeAsync(this, aErrorController, aOutputWindowController, aAsyncPackage, mCommandSet, CommandIds.kStopClang);

      if (null == SettingsCommand.Instance)
        await SettingsCommand.InitializeAsync(this, aAsyncPackage, mCommandSet, CommandIds.kSettingsId);
    }


    /// <summary>
    /// It is called immediately after every clang command execution.
    /// Set the running state to false.
    /// </summary>
    public void OnAfterClangCommand()
    {
      UIUpdater.Invoke(() =>
      {
        Running = false;
      });
    }


    #endregion


    #region Private Methods


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
    public void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action) => VsBuildRunning = true;


    /// <summary>
    /// Set the VS running build flag to false when the VS build finished.
    /// </summary>
    /// <param name="Scope"></param>
    /// <param name="Action"></param>
    public void OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
    {
      VsBuildRunning = false;
      OnBuildDoneClangCompile();
    }


    private void OnBuildDoneClangCompile()
    {
      if (false == CompileCommand.Instance.VsCompileFlag)
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
      CompileCommand.Instance.RunClangCompile(CommandIds.kCompileId);
      CompileCommand.Instance.VsCompileFlag = false;
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

      var tidyOption = SettingsProvider.GetSettingsPage(typeof(ClangTidyOptionsView)) as ClangTidyOptionsView;

      if (false == tidyOption.AutoTidyOnSave) // The clang-tidy on save option is disable 
        return;

      if (true == Running) // Clang compile/tidy command is running
        return;

      TidyCommand.Instance.RunClangTidy(CommandIds.kTidyFixId, true);
      mSaveCommandWasGiven = false;
    }


    private void BeforeSaveClangFormat(Document aDocument)
    {
      var clangFormatOptionPage = SettingsProvider.GetSettingsPage(typeof(ClangFormatOptionsView)) as ClangFormatOptionsView;

      if (false == clangFormatOptionPage.EnableFormatOnSave)
        return;

      if (false == Vsix.IsDocumentDirty(aDocument))
        return;

      if (false == FileHasExtension(aDocument.FullName, clangFormatOptionPage.FileExtensions))
        return;

      if (true == SkipFile(aDocument.FullName, clangFormatOptionPage.SkipFiles))
        return;

      var option = (SettingsProvider.GetSettingsPage(typeof(ClangFormatOptionsView)) as ClangFormatOptionsView).Clone();
      option.FallbackStyle = ClangFormatFallbackStyle.none;

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
      var generalOptions = SettingsProvider.GetSettingsPage(typeof(ClangGeneralOptionsView)) as ClangGeneralOptionsView;

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
