using ClangPowerTools.Commands.BackgroundTidy;
using ClangPowerTools.MVVM.LicenseValidation;
using ClangPowerTools.Views;
using Microsoft.VisualStudio.Shell;
using System;
using EnvDTE;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;
using System.Windows.Interop;
using Microsoft.Internal.VisualStudio.PlatformUI;

namespace ClangPowerTools.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  public sealed class SettingsCommand : BasicCommand
  {
    #region Properties

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static SettingsCommand Instance
    {
      get;
      private set;
    }

    #endregion


    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private SettingsCommand(CommandController aCommandController, OleMenuCommandService aCommandService, AsyncPackage aPackage, Guid aGuid, int aId)
      : base(aPackage, aGuid, aId)
    {
      if (null != aCommandService)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuItem = new OleMenuCommand(aCommandController.Execute, menuCommandID);
        aCommandService.AddCommand(menuItem);
      }
    }

    #endregion


    #region Public Methods


    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(CommandController aCommandController, AsyncPackage aPackage, Guid aGuid, int aId)
    {
      // Switch to the main thread - the call to AddCommand in SettingsCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(aPackage.DisposalToken);

      OleMenuCommandService commandService = await aPackage.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
      Instance = new SettingsCommand(aCommandController, commandService, aPackage, aGuid, aId);
    }


    public async Task ShowSettingsAsync()
    {
      bool activeLicense = await new PersonalLicenseValidator().ValidateAsync();
      SettingsView settingsView = new SettingsView(activeLicense);
      DTE dte = (DTE)ServiceProvider.GetService(typeof(DTE));
      WindowHelper.ShowModal(settingsView, (IntPtr) dte.MainWindow.HWnd);
    }


    #endregion

  }
}
