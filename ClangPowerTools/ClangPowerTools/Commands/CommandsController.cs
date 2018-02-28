using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ClangPowerTools
{
  public class CommandsController
  {
    #region Members

    private IServiceProvider mServiceProvider;
    private Dispatcher mDispatcher;
    private DTE2 mDte;
    //private ClangFormatPage mClangFormatPage;
    private Package mPackage;

    #endregion

    #region Constructor

    public CommandsController(IServiceProvider aServiceProvider, DTE2 aDte)
    {
      mDte = aDte;
      mServiceProvider = aServiceProvider;
      mDispatcher = HwndSource.FromHwnd((IntPtr)mDte.MainWindow.HWnd).RootVisual.Dispatcher;
    }

    #endregion

    #region Properties

    public bool Running { get; set; }
    public bool VsBuildRunning { get; set; }

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
        var itemsCollector = new ItemsCollector(mPackage);
        itemsCollector.CollectSelectedFiles(mDte, ActiveWindowProperties.GetProjectItemOfActiveWindow(mDte));

        if (!(sender is OleMenuCommand command))
          return;

        if (false == mDte.Solution.IsOpen)
          command.Enabled = false;

        else if (true == VsBuildRunning && command.CommandID.ID != CommandIds.kSettingsId)
          command.Enabled = false;

        else if (1 == itemsCollector.GetItems.Count && 
          (command.CommandID.ID == CommandIds.kCompileId || command.CommandID.ID == CommandIds.kTidyId) &&
          false == AutomationUtil.ContainLoadedItems(itemsCollector.GetItems))
        {
          command.Enabled = false;
        }
        else
        {
          command.Enabled = command.CommandID.ID != CommandIds.kStopClang ? !Running : Running;
          command.Visible = true;
        }

      }));
    }

    public void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action)
    {
      VsBuildRunning = true;
      ErrorsManager errorsManager = new ErrorsManager(mServiceProvider, mDte);
      errorsManager.Clear();
    }

    public void OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
    {
      VsBuildRunning = false;
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
