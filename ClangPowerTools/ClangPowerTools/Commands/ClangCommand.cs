﻿using ClangPowerTools.Builder;
using ClangPowerTools.Commands;
using ClangPowerTools.Events;
using ClangPowerTools.Script;
using ClangPowerTools.Services;
using ClangPowerTools.Tests;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools
{
  public abstract class ClangCommand : BasicCommand
  {
    #region Members

    protected ItemsCollector mItemsCollector;
    protected FilePathCollector mFilePahtCollector;
    protected static RunningProcesses mRunningProcesses = new RunningProcesses();
    protected List<string> mDirectoriesPath = new List<string>();
    private static bool stopCommand = false;

    //private Commands2 mCommand;

    private const string kVs15Version = "2017";
    private readonly Dictionary<string, string> mVsVersions = new Dictionary<string, string>
    {
      {"11.0", "2010"},
      {"12.0", "2012"},
      {"13.0", "2013"},
      {"14.0", "2015"},
      {"15.0", "2017"},
      {"16.0", "2019"}
    };

    private bool mMissingLLVM = false;
    private IVsHierarchy mHierarchy;

    public event EventHandler<VsHierarchyDetectedEventArgs> HierarchyDetectedEvent;
    public event EventHandler<CloseDataStreamingEventArgs> CloseDataStreamingEvent;
    public event EventHandler<ActiveDocumentEventArgs> ActiveDocumentEvent;

    public bool StopCommand
    {
      get { return stopCommand; }
      protected set { stopCommand = value; }
    }

    #endregion


    #region Properties

    public string Script { get; private set; }

    protected string VsEdition { get; set; }

    protected string VsVersion { get; set; }

    protected string WorkingDirectoryPath { get; set; }

    protected IVsHierarchy ItemHierarchy
    {
      get => mHierarchy;
      set
      {
        if (null == value)
          return;
        mHierarchy = value;
        OnFileHierarchyChanged(new VsHierarchyDetectedEventArgs(mHierarchy));
      }
    }


    #endregion


    #region Constructor


    public ClangCommand(AsyncPackage aPackage, Guid aGuid, int aId)
        : base(aPackage, aGuid, aId)
    {

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


    #region Public Methods

    public void OnMissingLLVMDetected(object sender, MissingLlvmEventArgs e)
    {
      mMissingLLVM = e.MissingLLVM;
    }

    #endregion

    #region Protected Methods

    protected void RunScript(int aCommandId)
    {
      string runModeParameters = GetRunModeParamaters();
      string genericParameters = GetGenericParamaters(aCommandId);

      if (mMissingLLVM)
        return;

      InvokeCommand(runModeParameters, genericParameters);
    }

    //Collect files
    protected IEnumerable<IItem> CollectItems(List<string> aAcceptedExtensionTypes = null, CommandUILocation commandUILocation = CommandUILocation.ContextMenu)
    {
      mItemsCollector = new ItemsCollector(aAcceptedExtensionTypes);
      switch (commandUILocation)
      {
        case CommandUILocation.Toolbar:
          mItemsCollector.CollectActiveProjectItem();
          SetActiveDocumentEvent();
          break;
        case CommandUILocation.ContextMenu:
          mItemsCollector.CollectSelectedItems();
          break;
      }
      return mItemsCollector.Items;
    }

    private void SetActiveDocumentEvent()
    {
      if (mItemsCollector.Items.Count == 0)
      {
        OnActiveFileCheck(new ActiveDocumentEventArgs(false));
      }
      else
      {
        OnActiveFileCheck(new ActiveDocumentEventArgs(true));
      }
    }

    protected async Task PrepareCommmandAsync(CommandUILocation commandUILocation)
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      DocumentsHandler.SaveActiveDocuments();

      if (!VsServiceProvider.TryGetService(typeof(DTE), out object dte))
        return;

      AutomationUtil.SaveDirtyProjects((dte as DTE2).Solution);
      CollectItems(ScriptConstants.kAcceptedFileExtensions, commandUILocation);
    }

    protected void OnActiveFileCheck(ActiveDocumentEventArgs e)
    {
      ActiveDocumentEvent?.Invoke(this, e);
    }

    protected void OnFileHierarchyChanged(VsHierarchyDetectedEventArgs e)
    {
      HierarchyDetectedEvent?.Invoke(this, e);
    }

    protected void OnDataStreamClose(CloseDataStreamingEventArgs e)
    {
      CloseDataStreamingEvent?.Invoke(this, e);
    }

    #endregion


    #region Private Methods

    private string GetGenericParamaters(int aCommandId)
    {
      IBuilder<string> genericScriptBuilder = new GenericScriptBuilder(VsEdition, VsVersion, aCommandId);
      genericScriptBuilder.Build();
      var genericParameters = genericScriptBuilder.GetResult();
      return genericParameters;
    }

    private static string GetRunModeParamaters()
    {
      IBuilder<string> runModeScriptBuilder = new RunModeScriptBuilder();
      runModeScriptBuilder.Build();
      var runModeParameters = runModeScriptBuilder.GetResult();
      return runModeParameters;
    }

    private void InvokeCommand(string runModeParameters, string genericParameters)
    {
      VsServiceProvider.TryGetService(typeof(SVsSolution), out object vsSolutionService);
      var vsSolution = vsSolutionService as IVsSolution;
      foreach (var item in mItemsCollector.Items)
      {
        if (StopCommand)
        {
          break;
        }

        IBuilder<string> itemRelatedScriptBuilder = new ItemRelatedScriptBuilder(item);
        itemRelatedScriptBuilder.Build();
        var itemRelatedParameters = itemRelatedScriptBuilder.GetResult();

        // From the first parameter is removed the last character which is mandatory "'"
        // and added to the end of the string to close the script
        Script = $"{runModeParameters.Remove(runModeParameters.Length - 1)} {itemRelatedParameters} {genericParameters}'";
        CommandTestUtility.ScriptCommand = Script;

        if (null != vsSolution)
          ItemHierarchy = AutomationUtil.GetItemHierarchy(vsSolution as IVsSolution, item);

        PowerShellWrapper.Invoke(Script, mRunningProcesses);
      }

      if (StopCommand)
      {
        OnDataStreamClose(new CloseDataStreamingEventArgs(true));
        StopCommand = false;
      }
      else
      {
        OnDataStreamClose(new CloseDataStreamingEventArgs(false));
      }
    }

    #endregion

  }
}
