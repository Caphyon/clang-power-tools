using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;

namespace ClangPowerTools
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class SettingsCommand
  {
    #region Members

    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 0x0103;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("498fdff5-5217-4da9-88d2-edad44ba3874");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly Package mPackage;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    private SettingsCommand(Package aPackage)
    {
      mPackage = aPackage ?? throw new ArgumentNullException("package");

      if (this.ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
      {
        var menuCommandID = new CommandID(CommandSet, CommandId);
        var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
        commandService.AddCommand(menuItem);
      }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static SettingsCommand Instance { get; private set; }

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
    public static void Initialize(Package package)
    {
      Instance = new SettingsCommand(package);
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
      mPackage.ShowOptionPage(typeof(GeneralOptions));
    }

    #endregion

  }
}
