using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ClangPowerTools
{
  public class CommandsController
  {
    #region Members

    private Dispatcher mDispatcher;

    #endregion

    #region Constructor

    public CommandsController(IServiceProvider aServiceProvider, DTE2 aDte)
    {
      mDispatcher = HwndSource.FromHwnd((IntPtr)aDte.MainWindow.HWnd).RootVisual.Dispatcher;
      Running = true;
    }

    #endregion

    #region Properties

    public bool Running { get; set; }

    #endregion

    #region Public Methods

    public void AfterExecute()
    {
      mDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
      {
        Running = false;
      }));
    }

    public void QueryCommandHandler(object sender, EventArgs e)
    {
      mDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
      {
        if (sender is OleMenuCommand command)
        {
          //MessageBox.Show(command.CommandID.ID.ToString());
          command.Enabled = !Running;
          command.Visible = true;
        }
      }));
    }

    #endregion

  }
}
