using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.Events;
using ClangPowerTools.Services;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerToolsShared.Commands
{
  public sealed class DiffCommand : ClangCommand
  {

    public static DiffCommand Instance
    {
      get;
      private set;
    }


    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DiffCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>

    private DiffCommand(OleMenuCommandService aCommandService, CommandController aCommandController,
      AsyncPackage aPackage, Guid aGuid, int aId)
        : base(aPackage, aGuid, aId)
    {
      if (null != aCommandService)
      {
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
      Instance = new DiffCommand(commandService, aCommandController, aPackage, aGuid, aId);
    }

    public async Task TidyDiffAsync(int aCommandId, CommandUILocation commandUILocation)
    {
      await PrepareCommmandAsync(commandUILocation);

      var clangTidyPath = Path.Combine(SettingsProvider.LlvmSettingsModel.PreinstalledLlvmPath, "clang-tidy.exe");
      FilePathCollector fileCollector = new FilePathCollector();
      var filesPath = fileCollector.Collect(mItemsCollector.Items).ToList();

      if (filesPath.Count == 1)
      {
        foreach (string path in filesPath)
        {
          if (StopCommandActivated)
            break;

          FileInfo file = new(path);
          var copyFile = Path.Combine(file.Directory.FullName, "_" + file.Name);
          File.Copy(file.FullName, copyFile, true);
          System.Diagnostics.Process process = new();
          process.StartInfo.FileName = clangTidyPath;
          process.StartInfo.CreateNoWindow = true;
          process.StartInfo.UseShellExecute = false;
          process.StartInfo.Arguments = $"-fix \"{copyFile}\"";
          process.Start();
          process.WaitForExit();
          DiffFilesUsingDefaultTool(copyFile, file.FullName);
          File.Delete(copyFile);
        }
      }
      if (StopCommandActivated)
      {
        StopCommandActivated = false;
      }

    }

    private static void DiffFilesUsingDefaultTool(string file1, string file2)
    {
      object args = $"\"{file1}\" \"{file2}\"";
      var dte = VsServiceProvider.GetService(typeof(DTE2)) as DTE2;
      dte.Commands.Raise(TidyConstants.ToolsDiffFilesCmd, TidyConstants.ToolsDiffFilesId, ref args, ref args);
    }

    #endregion
  }
}
