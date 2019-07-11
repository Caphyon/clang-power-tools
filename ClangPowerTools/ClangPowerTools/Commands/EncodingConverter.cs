using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClangPowerTools.MVVM.Helpers;
using ClangPowerTools.MVVM.ViewModels;
using ClangPowerTools.Properties;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class EncodingConverter : BasicCommand
  {
    #region Properties

    public static EncodingConverter Instance
    {
      get;
      private set;
    }

    #endregion

    #region Constructor
    private EncodingConverter(CommandController commandController, AsyncPackage package, OleMenuCommandService commandService, Guid guid, int id)
         : base(package, guid, id)
    {
      if (commandService != null)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuItem = new OleMenuCommand(commandController.Execute, menuCommandID);
        commandService.AddCommand(menuItem);
      }
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(CommandController commandController, AsyncPackage package, Guid guid, int id)
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

      OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
      Instance = new EncodingConverter(commandController, package, commandService, guid, id);
    }


    public async void RunEncodingConverter(int id, CommandUILocation commandUILocation)
    {
      //var dte = (DTE2)await ServiceProvider.GetServiceAsync(typeof(DTE));
      //string solutionDir = Path.GetDirectoryName(dte.Solution.FullName);
      var selectedFiles = GetSelectedFile(commandUILocation);
      if (selectedFiles == null)
      {
        return;
      }
      var encodingConverterViewModel = new EncodingConverterViewModel(selectedFiles);
      await encodingConverterViewModel.LoadData();

      var EncodingConverterWindow = WindowManager.CreateElementWindow(encodingConverterViewModel, Resources.EncodingConverterWindowTitle, "ClangPowerTools.MVVM.Views.EncodingConverterControl");

      if (encodingConverterViewModel.CloseAction == null)
      {
        encodingConverterViewModel.CloseAction = () => EncodingConverterWindow.Close();
      }

      EncodingConverterWindow.ShowDialog();
    }

    private List<IItem> GetSelectedFile(CommandUILocation commandUILocation)
    {
      if (commandUILocation == CommandUILocation.ContextMenu)
      {
        var itemsCollector = new ItemsCollector(ScriptConstants.kAcceptedFileExtensions);
        itemsCollector.CollectSelectedProjectItems();
        return itemsCollector.Items;
      }
      else if (commandUILocation == CommandUILocation.Toolbar)
      {
        var itemsCollector = new ItemsCollector(ScriptConstants.kAcceptedFileExtensions);
        itemsCollector.CollectActiveProjectItem();
        return itemsCollector.Items;
      }
      else
      {
        return null;
      }
    }

    #endregion
  }
}
