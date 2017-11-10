using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using EnvDTE80;

namespace ClangPowerTools
{
  public abstract class ClangCommand : BasicCommand
  {
    #region Members

    protected DTE2 mDte;
    protected string mVsEdition;
    protected string mVsVersion;
    protected const string kVs15Version = "2017";

    protected OutputManager mOutputManager;
    protected ErrorsManager mErrorsManager;

    protected CommandsController mCommandsController;
    protected ItemsCollector mItemsCollector;

    protected GeneralOptions mGeneralOptions;

    protected PowerShellWrapper mPowerShell = new PowerShellWrapper();
    protected ScriptBuiler mScriptBuilder;

    #endregion


    #region Constructor

    public ClangCommand(Package aPackage, Guid aGuid, int aId, DTE2 aDte, 
      string aEdition, string aVersion, CommandsController aCommandsController) 
        : base(aPackage, aGuid, aId)
    {
      mDte = aDte;
      mVsEdition = aEdition;
      mVsVersion = aVersion;
      mCommandsController = aCommandsController;

      mErrorsManager = new ErrorsManager(Package, mDte);
      mGeneralOptions = (GeneralOptions)Package.GetDialogPage(typeof(GeneralOptions));
    }

    #endregion

    #region Protected methods

    protected void RunScript(string aCommandName, TidyOptions mTidyOptions = null, TidyChecks mTidyChecks = null)
    {
      mScriptBuilder = new ScriptBuiler();
      mScriptBuilder.ConstructParameters(mGeneralOptions, mTidyOptions, mTidyChecks, mDte, mVsEdition, mVsVersion);
     
      mOutputManager = new OutputManager(mDte);
      InitPowerShell();
      ClearWindows(aCommandName);
      mOutputManager.AddMessage($"\n{OutputWindowConstants.kStart} {aCommandName}\n");
      foreach (var item in mItemsCollector.GetItems)
      {
        var script = mScriptBuilder.GetScript(item, item.GetName());
        mPowerShell.Invoke(script);
        if (mOutputManager.MissingLlvm)
        {
          mOutputManager.AddMessage(ErrorParserConstants.kMissingLlvmMessage);
          break;
        }
      }
      if (!mOutputManager.EmptyBuffer)
        mOutputManager.AddMessage(String.Join("\n", mOutputManager.Buffer));
      if (!mOutputManager.MissingLlvm)
      {
        mOutputManager.Show();
        mOutputManager.AddMessage($"\n{OutputWindowConstants.kDone} {aCommandName}\n");
      }
      if (mOutputManager.HasErrors)
        mErrorsManager.AddErrors(mOutputManager.Errors);
    }

    protected List<IItem> CollectSelectedItems()
    {
      mItemsCollector = new ItemsCollector(Package);
      mItemsCollector.CollectSelectedFiles(mDte, ActiveWindowProperties.GetProjectItemOfActiveWindow(mDte));
      return mItemsCollector.GetItems;
    }

    protected void LoadAllProjects()
    {
      if (kVs15Version != mVsVersion)
        return;
      Vs15SolutionLoader solutionLoader = new Vs15SolutionLoader(Package);
      solutionLoader.EnsureSolutionProjectsAreLoaded();
    }

    protected void SaveActiveDocuments() => mDte.Documents.SaveAll();

    #endregion

    #region Private Methods

    private void InitPowerShell()
    {
      mPowerShell = new PowerShellWrapper();
      mPowerShell.DataHandler += mOutputManager.OutputDataReceived;
      mPowerShell.DataErrorHandler += mOutputManager.OutputDataErrorReceived;
    }

    private void ClearWindows(string aCommandName)
    {
      mErrorsManager.Clear();
      mOutputManager.Clear();
      mOutputManager.Show();
    }

    #endregion

  }
}
