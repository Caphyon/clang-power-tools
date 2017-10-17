using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ClangPowerTools
{
  public class CommandsController
  {
    #region Members

    private OleMenuCommandService mCommandService;
    private List<CommandID> mCommandsId = new List<CommandID>();
    private Dispatcher mDispatcher;

    #endregion

    #region Constructor

    public CommandsController(IServiceProvider aServiceProvider, DTE2 aDte)
    {
      mCommandService = aServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
      mDispatcher = HwndSource.FromHwnd((IntPtr)aDte.MainWindow.HWnd).RootVisual.Dispatcher;
      Enable = true;
    }

    #endregion

    #region Properties

    private bool Enable { get; set; }

    #endregion

    #region Public Methods

    public void AddCommand(CommandID aCommandId) => mCommandsId.Add(aCommandId);

    #endregion

    #region Events

    //public void BeforeExecute(object sender, EventArgs e)
    //{
    //  mDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
    //  {
    //    foreach (var id in mCommandsId)
    //    {
    //      var command = mCommandService.FindCommand(id) as OleMenuCommand;
    //      mCommandService.RemoveCommand(command);
    //      if (null != command)
    //        command.Enabled = false;
    //      mCommandService.AddCommand(command);
    //    }
    //  }));
    //}

    public void BeforeExecute()
    {
      mDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
      {
        foreach (var id in mCommandsId)
        {
          var command = mCommandService.FindCommand(id) as OleMenuCommand;
          if (null != command)
            command.Enabled = false;
        }
      }));
    }

    //public void AfterExecute(object sender, EventArgs e)
    //{
    //  mDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
    //  {
    //    foreach (var id in mCommandsId)
    //    {
    //      var command = mCommandService.FindCommand(id) as OleMenuCommand;
    //      mCommandService.RemoveCommand(command);
    //      if (null != command)
    //        command.Enabled = true;
    //      mCommandService.AddCommand(command);
    //    }
    //  }));
    //}

    public void AfterExecute()
    {
      mDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
      {
        foreach (var id in mCommandsId)
        {
          var command = mCommandService.FindCommand(id) as OleMenuCommand;
          if (null != command)
            command.Enabled = true;
        }
      }));
    }

    #endregion

  }
}
