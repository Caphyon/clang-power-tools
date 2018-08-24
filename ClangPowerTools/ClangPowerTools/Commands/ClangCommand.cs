﻿using ClangPowerTools.Builder;
using ClangPowerTools.DialogPages;
using ClangPowerTools.Output;
using ClangPowerTools.Script;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public abstract class ClangCommand : BasicCommand
  {
    #region Members


    protected static CommandsController mCommandsController = null;
    protected ItemsCollector mItemsCollector;
    protected FilePathCollector mFilePahtCollector;
    protected static RunningProcesses mRunningProcesses = new RunningProcesses();
    protected List<string> mDirectoriesPath = new List<string>();
    protected ClangGeneralOptionsView mGeneralOptions;

    private Commands2 mCommand;
    private IVsSolution mSolution;

    private ErrorWindowController mErrorWindow;
    private OutputWindowController mOutputWindow;

    private PowerShellWrapper mPowerShell = new PowerShellWrapper();
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


    public ClangCommand(CommandsController aCommandsController, ErrorWindowController aErrorWindow, OutputWindowController aOutputWindow,
      IVsSolution aSolution, DTE2 aDte, AsyncPackage aPackage, Guid aGuid, int aId)
        : base(aDte, aPackage, aGuid, aId)
    {
      if (null == mCommandsController)
        mCommandsController = aCommandsController;

      mErrorWindow = aErrorWindow;
      mOutputWindow = aOutputWindow;
      mGeneralOptions = (ClangGeneralOptionsView)aPackage.GetDialogPage(typeof(ClangGeneralOptionsView));

      mSolution = aSolution;
      mCommand = DTEObj.Commands as Commands2;
      VsEdition = DTEObj.Edition;
      mVsVersions.TryGetValue(DTEObj.Version, out string version);
      VsVersion = version;
    }

    #endregion


    #region Methods


    #region Public Methods


    public virtual void OnBeforeSave(object sender, Document aDocument) { }

    public virtual void CommandEventsBeforeExecute(string aGuid, int aId, object aCustomIn, object aCustomOut, ref bool aCancelDefault) { }

    public virtual void OnBuildDone(vsBuildScope Scope, vsBuildAction Action) { }


    #endregion


    #region Protected methods

    protected void RunScript(string aCommandName, ClangTidyOptionsView mTidyOptions = null, ClangTidyPredefinedChecksOptionsView mTidyChecks = null,
      ClangTidyCustomChecksOptionsView mTidyCustomChecks = null, ClangFormatOptionsView aClangFormatView = null, bool aTidyFixFlag = false)
    {
      try
      {
        DTEObj.Solution.SaveAs(DTEObj.Solution.FullName);

        IBuilder<string> runModeScriptBuilder = new RunModeScriptBuilder();
        runModeScriptBuilder.Build();
        var runModeParameters = runModeScriptBuilder.GetResult();

        IBuilder<string> genericScriptBuilder = new GenericScriptBuilder(mGeneralOptions, mTidyOptions, mTidyChecks,
          mTidyCustomChecks, aClangFormatView, DTEObj, VsEdition, VsVersion, aTidyFixFlag);
        genericScriptBuilder.Build();
        var genericParameters = genericScriptBuilder.GetResult();

        string solutionPath = DTEObj.Solution.FullName;

        InitPowerShell();
        ClearWindows();
        mOutputWindow.Write($"\n{OutputWindowConstants.kStart} {aCommandName}\n");

        StatusBarHandler.Status(aCommandName + " started...", 1, vsStatusAnimation.vsStatusAnimationBuild, 1);

        foreach (var item in mItemsCollector.GetItems)
        {
          if (!mCommandsController.Running)
            break;

          IBuilder<string> itemRelatedScriptBuilder = new ItemRelatedScriptBuilder(item);
          itemRelatedScriptBuilder.Build();
          var itemRelatedParameters = itemRelatedScriptBuilder.GetResult();

          // From the first parameter is removed the last character which is mandatory "'"
          // and added to the end of the string to close the script
          var script = $"{runModeParameters.Remove(runModeParameters.Length - 1)} {itemRelatedParameters} {genericParameters}'";

          mOutputWindow.Hierarchy = AutomationUtil.GetItemHierarchy(mSolution, item);
          var process = mPowerShell.Invoke(script, mRunningProcesses);

          if (mOutputWindow.MissingLlvm)
          {
            mOutputWindow.Write(ErrorParserConstants.kMissingLlvmMessage);
            break;
          }
        }

        if (!mOutputWindow.MissingLlvm)
        {
          mOutputWindow.Show();
          mOutputWindow.Write($"\n{OutputWindowConstants.kDone} {aCommandName}\n");
        }

        if (mOutputWindow.HasErrors)
          mErrorWindow.AddErrors(mOutputWindow.Errors);
      }
      catch (Exception)
      {
        mOutputWindow.Show();
        mOutputWindow.Write($"\n{OutputWindowConstants.kDone} {aCommandName}\n");
      }
      finally
      {
        StatusBarHandler.Status(aCommandName + " finished", 0, vsStatusAnimation.vsStatusAnimationBuild, 0);
      }
    }

    protected IEnumerable<IItem> CollectSelectedItems(List<string> aAcceptedExtensionTypes = null)
    {
      mItemsCollector = new ItemsCollector(aAcceptedExtensionTypes);
      mItemsCollector.CollectSelectedFiles(DTEObj, ActiveWindowProperties.GetProjectItemOfActiveWindow(DTEObj));
      return mItemsCollector.GetItems;
    }

    protected string GetCommandName(string aGuid, int aId)
    {
      try
      {
        if (null == aGuid)
          return string.Empty;

        if (null == mCommand)
          return string.Empty;

        Command cmd = mCommand.Item(aGuid, aId);
        if (null == cmd)
          return string.Empty;

        return cmd.Name;
      }
      catch (Exception) { }

      return string.Empty;
    }

    #endregion


    #region Private Methods

    private void InitPowerShell()
    {
      mPowerShell = new PowerShellWrapper();
      mPowerShell.DataHandler += mOutputWindow.OutputDataReceived;
      mPowerShell.DataErrorHandler += mOutputWindow.OutputDataErrorReceived;
      mPowerShell.ExitedHandler += mOutputWindow.ClosedDataConnection;
    }

    private void ClearWindows()
    {
      mErrorWindow.Clear();
      mOutputWindow.Clear();
      mOutputWindow.Show();
    }

    #endregion

    #endregion


  }
}
