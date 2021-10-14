﻿using ClangPowerTools.Helpers;
using ClangPowerTools.Services;
using ClangPowerTools.SilentFile;
using ClangPowerToolsShared.MVVM.Views.ToolWindows;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  public sealed class TidyCommand : ClangCommand
  {

    #region Properties
    private readonly AsyncPackage package;

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    /// 
    public static TidyCommand Instance
    {
      get;
      private set;
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="TidyCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>

    private TidyCommand(OleMenuCommandService aCommandService, CommandController aCommandController,
      AsyncPackage aPackage, Guid aGuid, int aId)
        : base(aPackage, aGuid, aId)
    {
      if (null != aCommandService)
      {
        package = aPackage;
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuCommand = new OleMenuCommand(aCommandController.Execute, menuCommandID);
        menuCommand.BeforeQueryStatus += aCommandController.OnBeforeClangCommand;
        menuCommand.Enabled = true;
        aCommandService.AddCommand(menuCommand);
      }
    }


    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(CommandController aCommandController,
      AsyncPackage aPackage, Guid aGuid, int aId)
    {
      // Switch to the main thread - the call to AddCommand in TidyCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(aPackage.DisposalToken);

      OleMenuCommandService commandService = await aPackage.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
      Instance = new TidyCommand(commandService, aCommandController, aPackage, aGuid, aId);

    }

    public async Task ShowTidyToolWindowAsync()
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      ToolWindowPane window = await package.ShowToolWindowAsync(
      typeof(TidyToolWindow),
      0,
      create: true,
      cancellationToken: package.DisposalToken);
      var tidyToolWindow = (TidyToolWindow)window;

      FilePathCollector fileCollector = new FilePathCollector();
      var filesPath = fileCollector.Collect(mItemsCollector.OriginalItems).ToList();
      tidyToolWindow.UpdateToolWindow(filesPath);
    }

    public async Task RunClangTidyAsync(int aCommandId, CommandUILocation commandUILocation, List<string> paths = null)
    {

      await PrepareCommmandAsync(commandUILocation);

      if (paths != null)
      {
        mItemsCollector.Items = new List<IItem>();
        foreach (var path in paths)
        {
          mItemsCollector.Items.Add(mItemsCollector.OriginalItems.Where(a => a.GetPath() == path).FirstOrDefault());
        }
      }

      await Task.Run(() =>
      {
        lock (mutex)
        {
          try
          {
            using var silentFileController = new SilentFileChangerController();
            using var fileChangerWatcher = new FileChangerWatcher();

            var tidySettings = SettingsProvider.TidySettingsModel;

            if (CommandIds.kTidyFixId == aCommandId || tidySettings.TidyOnSave)
            {
              fileChangerWatcher.OnChanged += FileOpener.Open;

              var dte2 = VsServiceProvider.GetService(typeof(DTE2)) as DTE2;
              string solutionFolderPath = SolutionInfo.IsOpenFolderModeActive() ?
                dte2.Solution.FullName : dte2.Solution.FullName
                                          .Substring(0, dte2.Solution.FullName.LastIndexOf('\\'));

              fileChangerWatcher.Run(solutionFolderPath);
              var filesPath = new List<string>();

              if (paths is null)
              {
                FilePathCollector fileCollector = new FilePathCollector();
                filesPath = fileCollector.Collect(mItemsCollector.Items).ToList();
              }
              else
              {
                filesPath = paths;
              }

              silentFileController.SilentFiles(filesPath);
              silentFileController.SilentFiles(dte2.Documents);

            }

            if (tidySettings.DetectClangTidyFile && !mItemsCollector.IsEmpty)
            {
              // Check for .clang-tidy config file
              if (FileSystem.SearchAllTopDirectories(mItemsCollector.Items[0].GetPath(), FileSystem.ConfigClangTidyFileName))
                tidySettings.UseChecksFrom = ClangTidyUseChecksFrom.TidyFile;
              else
                tidySettings.UseChecksFrom = ClangTidyUseChecksFrom.PredefinedChecks;

              var settingsHandlder = new SettingsHandler();
              settingsHandlder.SaveSettings();
            }

            RunScript(aCommandId, false);
          }
          catch (Exception exception)
          {
            VsShellUtilities.ShowMessageBox(AsyncPackage, exception.Message, "Error",
              OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
          }
        }
      });
    }


    #endregion

    #region Private Methods

    private static void DiffFilesUsingDefaultTool(string file1, string file2)
    {
      object args = $"\"{file1}\" \"{file2}\"";
      var dte = VsServiceProvider.GetService(typeof(DTE2)) as DTE2;
      dte.Commands.Raise(TidyConstants.ToolsDiffFilesCmd, TidyConstants.ToolsDiffFilesId, ref args, ref args);
    }

    #endregion
  }
}
