using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;

namespace ClangPowerTools
{
  public class CommandsController
  {
    #region Members

    private IServiceProvider mServiceProvider;
    private DTE2 mDte;

    #endregion

    #region Constructor

    public CommandsController(IServiceProvider aServiceProvider, DTE2 aDte)
    {
      mDte = aDte;
      mServiceProvider = aServiceProvider;
    }

    #endregion

    #region Properties

    public bool Running { get; set; }
    public bool VsBuildRunning { get; set; }

    #endregion

    #region Public Methods

    public void AfterExecute()
    {
      DispatcherHandler.Invoke(() =>
      {
        Running = false;
      });
    }

    public void QueryCommandHandler(object sender, EventArgs e)
    {
      DispatcherHandler.Invoke(() =>
      {
        //var itemsCollector = new ItemsCollector(mPackage);
        //itemsCollector.CollectSelectedFiles(mDte, ActiveWindowProperties.GetProjectItemOfActiveWindow(mDte));

        if (!(sender is OleMenuCommand command))
          return;

        if (false == mDte.Solution.IsOpen)
          command.Enabled = false;

        else if (true == VsBuildRunning && command.CommandID.ID != CommandIds.kSettingsId)
          command.Enabled = false;

        //else if (1 == itemsCollector.GetItems.Count && 
        //  (command.CommandID.ID == CommandIds.kCompileId || command.CommandID.ID == CommandIds.kTidyId) &&
        //  false == AutomationUtil.ContainLoadedItems(itemsCollector.GetItems))
        //{
        //  command.Enabled = false;
        //}
        else
        {
          command.Enabled = command.CommandID.ID != CommandIds.kStopClang ? !Running : Running;
          command.Visible = true;
        }
      });
     
    }

    public void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action)
    {
      VsBuildRunning = true;
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
