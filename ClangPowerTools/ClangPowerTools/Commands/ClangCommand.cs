using ClangPowerTools.Builder;
using ClangPowerTools.Output;
using ClangPowerTools.Script;
using ClangPowerTools.Services;
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

    //private Commands2 mCommand;

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


    public ClangCommand(CommandsController aCommandsController, ErrorWindowController aErrorWindow,
      OutputWindowController aOutputWindow, AsyncPackage aPackage, Guid aGuid, int aId)
        : base(aPackage, aGuid, aId)
    {
      if (null == mCommandsController)
        mCommandsController = aCommandsController;

      mErrorWindow = aErrorWindow;
      mOutputWindow = aOutputWindow;

      if (VsServiceProvider.TryGetService(typeof(DTE), out object dte))
      {
        var dte2 = dte as DTE2;
        //mCommand = dte2.Commands as Commands2;
        VsEdition = dte2.Edition;
        mVsVersions.TryGetValue(dte2.Version, out string version);
        VsVersion = version;
      }
    }

    #endregion


    #region Methods


    #region Protected methods

    protected void RunScript(int aCommandId)
    {
      try
      {
        var dte = VsServiceProvider.GetService(typeof(DTE)) as DTE2;
        dte.Solution.SaveAs(dte.Solution.FullName);

        IBuilder<string> runModeScriptBuilder = new RunModeScriptBuilder();
        runModeScriptBuilder.Build();
        var runModeParameters = runModeScriptBuilder.GetResult();

        IBuilder<string> genericScriptBuilder = new GenericScriptBuilder(VsEdition, VsVersion, aCommandId);
        genericScriptBuilder.Build();
        var genericParameters = genericScriptBuilder.GetResult();

        string solutionPath = dte.Solution.FullName;

        InitPowerShell();
        ClearWindows();
        mOutputWindow.Write($"\n{OutputWindowConstants.kStart} {OutputWindowConstants.kCommandsNames[aCommandId]}\n");

        StatusBarHandler.Status(OutputWindowConstants.kCommandsNames[aCommandId] + " started...", 1, vsStatusAnimation.vsStatusAnimationBuild, 1);

        VsServiceProvider.TryGetService(typeof(SVsSolution), out object vsSolutionService);
        var vsSolution = vsSolutionService as IVsSolution;

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

          if (null != vsSolution)
            mOutputWindow.Hierarchy = AutomationUtil.GetItemHierarchy(vsSolution as IVsSolution, item);

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
          mOutputWindow.Write($"\n{OutputWindowConstants.kDone} {OutputWindowConstants.kCommandsNames[aCommandId]}\n");
        }

        if (mOutputWindow.HasErrors)
          mErrorWindow.AddErrors(mOutputWindow.Errors);
      }
      catch (Exception)
      {
        mOutputWindow.Show();
        mOutputWindow.Write($"\n{OutputWindowConstants.kDone} {OutputWindowConstants.kCommandsNames[aCommandId]}\n");
      }
      finally
      {
        StatusBarHandler.Status(OutputWindowConstants.kCommandsNames[aCommandId] + " finished", 0, vsStatusAnimation.vsStatusAnimationBuild, 0);
        foreach (var process in mRunningProcesses.GetProcesses)
          process.Dispose();
      }
    }

    protected IEnumerable<IItem> CollectSelectedItems(bool aClangFormatFlag = false, List<string> aAcceptedExtensionTypes = null)
    {
      mItemsCollector = new ItemsCollector(aAcceptedExtensionTypes);
      mItemsCollector.CollectSelectedFiles(ActiveWindowProperties.GetProjectItemOfActiveWindow(), aClangFormatFlag);
      return mItemsCollector.GetItems;
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
