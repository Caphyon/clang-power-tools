using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ClangPowerTools
{
  public class CommandsController
  {
    #region Members

    private Dispatcher mDispatcher;
    private DTE2 mDte;
    #endregion

    #region Constructor

    public CommandsController(IServiceProvider aServiceProvider, DTE2 aDte)
    {
      mDte = aDte;
      mDispatcher = HwndSource.FromHwnd((IntPtr)mDte.MainWindow.HWnd).RootVisual.Dispatcher;
    }

    #endregion

    #region Properties

    public bool Running { get; set; }

    #endregion

    #region Public Methods

    public void AfterExecute()
    {
      mDispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
      {
        Running = false;
      }));
    }

    public void QueryCommandHandler(object sender, EventArgs e)
    {
      mDispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
      {
        if (!(sender is OleMenuCommand command))
          return;
        if (!mDte.Solution.IsOpen)
          command.Enabled = false;
        else
        {
          command.Enabled = !Running;
          command.Visible = true;
        }
      }));
    }

    #endregion

  }
}
