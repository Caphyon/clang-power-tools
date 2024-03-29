﻿using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerToolsShared.MVVM.Views.ToolWindows;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerToolsShared.Commands
{
  public class FindViewMenuCommand : ClangCommand
  {
    private readonly AsyncPackage package;
    public static FindViewMenuCommand Instance
    {
      get;
      private set;
    }

    protected FindViewMenuCommand(OleMenuCommandService aCommandService, CommandController aCommandController,
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

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(CommandController aCommandController,
      AsyncPackage aPackage, Guid aGuid, int aId)
    {
      // Switch to the main thread - the call to AddCommand in CompileCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(aPackage.DisposalToken);

      OleMenuCommandService commandService = await aPackage.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
      Instance = new FindViewMenuCommand(commandService, aCommandController, aPackage, aGuid, aId);
    }

    public async Task FindAsync(CommandUILocation commandUILocation)
    {
      await PrepareCommmandAsync(commandUILocation);

      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
      ToolWindowPane window = await package.ShowToolWindowAsync(
      typeof(FindToolWindow),
      0,
      create: true,
      cancellationToken: package.DisposalToken);
      var findToolWindow = (FindToolWindow)window;
      ItemsCollector itemsCollector = new ItemsCollector();
      itemsCollector.CollectSelectedProjectItems();
      FilePathCollector fileCollector = new FilePathCollector();
      var paths = fileCollector.Collect(itemsCollector.Items).ToList();
      if (findToolWindow != null)
        findToolWindow.OpenFindToolWindow(paths);
    }
  }
}
