using ClangPowerTools.DialogPages;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ClangPowerTools
{
  public class CommandsController
  {
    #region Members

    private Dispatcher mDispatcher;
    private DTE2 mDte;
    //private ClangFormatPage mClangFormatPage;
    //private Package mPackage;

    #endregion

    #region Constructor

    public CommandsController(Package aPackage, DTE2 aDte)
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
          command.Enabled = command.CommandID.ID != CommandIds.kStopClang ? !Running : Running;
          command.Visible = true;
        }

          //if (CommandIds.kClangFormat == command.CommandID.ID)
          //{
          //  if (Running)
          //    return;

          //  var fileExtensions = mClangFormatPage.FileExtensions
          //    .ToLower()
          //    .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
          //    .ToList();

          //  var itemsCollector = new ItemsCollector(mPackage, fileExtensions);
          //  itemsCollector.CollectSelectedFiles(mDte, ActiveWindowProperties.GetProjectItemOfActiveWindow(mDte));

          //  if(false == ContainsAcceptedFiles(itemsCollector.GetItems, fileExtensions) )
          //    command.Enabled = false;
          //  else
          //    command.Enabled = true;
          //}

        }));
    }

    #endregion

    #region Private methods

    //private bool ContainsAcceptedFiles(List<IItem> aItems, List<string> aFileExtensions)
    //{
    //  foreach (var item in aItems)
    //  {
    //    if (!(item.GetObject() is ProjectItem))
    //      return false;

    //    var itemName = (item.GetObject() as ProjectItem).Name;

    //    var extension = Path.GetExtension((item.GetObject() as ProjectItem).Name).ToLower();
    //    if (aFileExtensions.Contains(extension))
    //      return true;
    //  }
    //  return false;
    //}

    #endregion

  }
}
