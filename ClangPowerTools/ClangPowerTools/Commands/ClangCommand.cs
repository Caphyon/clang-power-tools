using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using EnvDTE80;

namespace ClangPowerTools
{
  public abstract class ClangCommand : BasicCommand
  {
    #region Members

    private const string kVs15Version = "2017";
    private Dictionary<string, string> mVsVersions = new Dictionary<string, string>
    {
      {"11.0", "2010"},
      {"12.0", "2012"},
      {"13.0", "2013"},
      {"14.0", "2015"},
      {"15.0", "2017"}
    };

    protected OutputManager mOutputManager;
    protected ErrorsManager mErrorsManager;

    protected static CommandsController mCommandsController = null;
    protected ItemsCollector mItemsCollector;
    protected GeneralOptions mGeneralOptions;

    protected PowerShellWrapper mPowerShell = new PowerShellWrapper();
    protected ScriptBuiler mScriptBuilder;

    #endregion

    #region Properties

    protected string VsEdition { get; set; }
    protected string VsVersion { get; set; }

    #endregion

    #region Constructor

    public ClangCommand(Package aPackage, Guid aGuid, int aId) : base(aPackage, aGuid, aId)
    {
      VsEdition = DTEObj.Edition;
      mVsVersions.TryGetValue(DTEObj.Version, out string version);
      VsVersion = version;

      if( null == mCommandsController )
        mCommandsController = new CommandsController(ServiceProvider, DTEObj);

      mErrorsManager = new ErrorsManager(Package, DTEObj);
      mGeneralOptions = (GeneralOptions)Package.GetDialogPage(typeof(GeneralOptions));
    }

    #endregion

    #region Protected methods

    protected void RunScript(string aCommandName, TidyOptions mTidyOptions = null, TidyChecks mTidyChecks = null)
    {
      mScriptBuilder = new ScriptBuiler();
      mScriptBuilder.ConstructParameters(mGeneralOptions, mTidyOptions, mTidyChecks, 
        DTEObj, VsEdition, VsVersion);

      mOutputManager = new OutputManager(DTEObj);
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
      mItemsCollector.CollectSelectedFiles(DTEObj, ActiveWindowProperties.GetProjectItemOfActiveWindow(DTEObj));
      return mItemsCollector.GetItems;
    }

    protected void LoadAllProjects()
    {
      if (kVs15Version != VsVersion)
        return;
      Vs15SolutionLoader solutionLoader = new Vs15SolutionLoader(Package);
      solutionLoader.EnsureSolutionProjectsAreLoaded();
    }

    protected void SaveActiveDocuments() => DTEObj.Documents.SaveAll();

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
