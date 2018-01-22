using System;
using System.Collections.Generic;
using ClangPowerTools.DialogPages;
using ClangPowerTools.Script;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace ClangPowerTools
{
  public abstract class ClangCommand : BasicCommand
  {
    #region Members

    protected static CommandsController mCommandsController = null;
    protected ItemsCollector mItemsCollector;
    protected static RunningProcesses mRunningProcesses = new RunningProcesses();
    protected List<string> mDirectoriesPath = new List<string>();
    protected static OutputManager mOutputManager;
    protected GeneralOptions mGeneralOptions;

    private ErrorsManager mErrorsManager;
    private PowerShellWrapper mPowerShell = new PowerShellWrapper();
    private ClangCompileTidyScript mCompileTidyScriptBuilder;
    private ClangFormatScript mClangFormatScriptBuilder;
    private const string kVs15Version = "2017";
    private Dictionary<string, string> mVsVersions = new Dictionary<string, string>
    {
      {"11.0", "2010"},
      {"12.0", "2012"},
      {"13.0", "2013"},
      {"14.0", "2015"},
      {"15.0", "2017"}
    };

    #endregion

    #region Properties

    protected string VsEdition { get; set; }
    protected string VsVersion { get; set; }
    protected string WorkingDirectoryPath { get; set; }

    #endregion

    #region Constructor

    public ClangCommand(Package aPackage, Guid aGuid, int aId) : base(aPackage, aGuid, aId)
    {
      try
      {
        VsEdition = DTEObj.Edition;
        mVsVersions.TryGetValue(DTEObj.Version, out string version);
        VsVersion = version;

    //  mRunningProcesses = new RunningProcesses();
    
      if (null == mCommandsController)
        mCommandsController = new CommandsController(ServiceProvider, DTEObj);

        mErrorsManager = new ErrorsManager(Package, DTEObj);
        mGeneralOptions = (GeneralOptions)Package.GetDialogPage(typeof(GeneralOptions));
      }
      catch (Exception)
      {
      }
    }

    #endregion

    #region Protected methods


    protected void RunScript(string aCommandName, bool aForceTidyToFix, TidyOptions mTidyOptions = null, 
      TidyChecks mTidyChecks = null, TidyCustomChecks mTidyCustomChecks = null, ClangFormatPage aClangFormat = null)
    {
      try
      {
        mCompileTidyScriptBuilder = new ClangCompileTidyScript();
        mCompileTidyScriptBuilder.ConstructParameters(mGeneralOptions, mTidyOptions, mTidyChecks,
          mTidyCustomChecks, aClangFormat, DTEObj, VsEdition, VsVersion, aForceTidyToFix);

        string solutionPath = DTEObj.Solution.FullName;

        mOutputManager = new OutputManager(DTEObj);
        InitPowerShell();
        ClearWindows();
        mOutputManager.AddMessage($"\n{OutputWindowConstants.kStart} {aCommandName}\n");
        foreach (var item in mItemsCollector.GetItems)
        {
          var script = mCompileTidyScriptBuilder.GetScript(item, solutionPath);
          if (!mCommandsController.Running)
            break;

          var process = mPowerShell.Invoke(script, mRunningProcesses);
          
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
      catch (Exception)
      {
      }
    }

    protected List<IItem> CollectSelectedItems(string aClangFormatExtensions = null, string aSkipFiles = null)
    {
      mItemsCollector = new ItemsCollector(Package, aClangFormatExtensions, aSkipFiles);
      mItemsCollector.CollectSelectedFiles(DTEObj, ActiveWindowProperties.GetProjectItemOfActiveWindow(DTEObj));
      return mItemsCollector.GetItems;
    }

    #endregion

    #region Private Methods

    private void InitPowerShell()
    {
      try
      {
        mPowerShell = new PowerShellWrapper();
        mPowerShell.DataHandler += mOutputManager.OutputDataReceived;
        mPowerShell.DataErrorHandler += mOutputManager.OutputDataErrorReceived;
      }
      catch (Exception)
      {
      }
    }

    private void ClearWindows()
    {
      try
      {
        mErrorsManager.Clear();
        mOutputManager.Clear();
        mOutputManager.Show();
      }
      catch (Exception)
      {
      }
    }

    #endregion

  }
}
