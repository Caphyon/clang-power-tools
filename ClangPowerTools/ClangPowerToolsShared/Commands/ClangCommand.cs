using ClangPowerTools.CMake;
using ClangPowerTools.Commands;
using ClangPowerTools.Events;
using ClangPowerTools.Helpers;
using ClangPowerTools.IgnoreActions;
using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools
{
  public abstract class ClangCommand : BasicCommand
  {
    #region Members

    public static RunningProcesses runningProcesses = new RunningProcesses();

    protected ItemsCollector mItemsCollector;
    protected FilePathCollector mFilePahtCollector;
    private readonly Dictionary<string, string> mVsVersions = new Dictionary<string, string>
    {
      {"11.0", "2010"},
      {"12.0", "2012"},
      {"13.0", "2013"},
      {"14.0", "2015"},
      {"15.0", "2017"},
      {"16.0", "2019"},
      {"17.0", "2022"}
    };

    private IVsHierarchy mHierarchy;

    // private object 

    public event EventHandler<VsHierarchyDetectedEventArgs> HierarchyDetectedEvent;
    public event EventHandler<CloseDataStreamingEventArgs> CloseDataStreamingEvent;
    public event EventHandler<ActiveDocumentEventArgs> ActiveDocumentEvent;

    public event EventHandler<ClangCommandMessageEventArgs> IgnoredItemsEvent;

    protected static bool StopCommandActivated { get; set; } = false;

    protected static object mutex = new object();

    public RunningProcesses GetClangProcesses => runningProcesses;

    #endregion


    #region Properties

    public string Script { get; private set; }

    protected string VsEdition { get; set; }

    protected string VsVersion { get; set; }

    protected string WorkingDirectoryPath { get; set; }

    public List<string> DirectoryPaths { get; set; } = new List<string>();

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

      if (VsServiceProvider.TryGetService(typeof(DTE2), out object dte))
      {
        var dte2 = dte as DTE2;
        VsEdition = dte2.Edition;
        mVsVersions.TryGetValue(dte2.Version, out string version);
        VsVersion = version;
      }
    }

    #endregion

    #region Protected Methods


    protected void RunScript(int aCommandId, bool jsonCompilationDbActive, List<string> paths = null)
    {
      string runModeParameters = ScriptGenerator.GetRunModeParamaters();
      string genericParameters = ScriptGenerator.GetGenericParamaters(aCommandId, VsEdition, VsVersion, jsonCompilationDbActive);

      CMakeBuilder cMakeBuilder = new CMakeBuilder();
      cMakeBuilder.Build();

      if (jsonCompilationDbActive)
        ExportJsonCompilationDatabase(runModeParameters, genericParameters);
      else
        Compile(runModeParameters, genericParameters, aCommandId, paths);

      cMakeBuilder.ClearBuildCashe();
    }

    //Collect files
    protected IEnumerable<IItem> CollectItemsDependingOnCommandLocation(
      CommandUILocation commandUILocation = CommandUILocation.ContextMenu, bool jsonCompilationDbActive = false)
    {
      mItemsCollector = new ItemsCollector(jsonCompilationDbActive);
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

    protected async Task PrepareCommmandAsync(CommandUILocation commandUILocation, bool jsonCompilationDbActive = false)
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

      DocumentHandler.SaveActiveDocuments();

      if (!VsServiceProvider.TryGetService(typeof(DTE2), out object dte))
        return;

      AutomationUtil.SaveDirtyProjects((dte as DTE2).Solution);
      CollectItemsDependingOnCommandLocation(commandUILocation, jsonCompilationDbActive);
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

    protected void OnIgnoreItem(ClangCommandMessageEventArgs e)
    {
      IgnoredItemsEvent?.Invoke(this, e);
    }

    #endregion

    #region Private Methods

    private void Compile(string runModeParameters, string genericParameters, int commandId, List<string> paths = null)
    {
      var vsSolution = SolutionInfo.IsOpenFolderModeActive() == false ?
        (IVsSolution)VsServiceProvider.GetService(typeof(SVsSolution)) : null;

      try
      {
        var tempClangTidyFilePath = string.Empty;
        foreach (var item in mItemsCollector.Items)
        {
          if (StopCommandActivated)
            break;

          var ignoreItem = new IgnoreItem();

          if (ignoreItem.Check(item, paths))
          {
            OnIgnoreItem(new ClangCommandMessageEventArgs(ignoreItem.IgnoreCompileOrTidyMessage, false));
            continue;
          }

          if (commandId == CommandIds.kTidyId || commandId == CommandIds.kTidyFixId ||
            commandId == CommandIds.kTidyToolbarId || commandId == CommandIds.kTidyFixToolbarId)
          {
            tempClangTidyFilePath = CreateTemporaryFileForTidy(item);
          }

          var itemRelatedParameters = string.Empty;
          if (item is CurrentProject || item is Project || item is Solution)
          {
            itemRelatedParameters = ScriptGenerator.GetItemRelatedParameters(item);
          }
          else if (item is CurrentProjectItem || item is ProjectItem)
          {
            itemRelatedParameters = ScriptGenerator.GetItemRelatedParameters(mItemsCollector.Items);
          }

          // From the first parameter is removed the last character which is mandatory "'"
          // and added to the end of the string to close the script escaping command
          Script = JoinUtility.Join(" ", runModeParameters.Remove(runModeParameters.Length - 1), itemRelatedParameters, genericParameters, "'");

          ItemHierarchy = vsSolution != null ? AutomationUtil.GetItemHierarchy(vsSolution, item) : null;

          PowerShellWrapper.Invoke(Script, runningProcesses);
          if (item is ProjectItem || item is CurrentProjectItem)
          {
            break;
          }
        }

        if (StopCommandActivated)
        {
          OnDataStreamClose(new CloseDataStreamingEventArgs(true));
          StopCommandActivated = false;
        }
        else
        {
          if (File.Exists(tempClangTidyFilePath))
          {
            File.Delete(tempClangTidyFilePath);
          }
          OnDataStreamClose(new CloseDataStreamingEventArgs(false));
        }

      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private string CreateTemporaryFileForTidy(IItem item)
    {
      var directoryPath = Directory.GetParent(item.GetPath()).FullName;
      StopCommand.Instance.DirectoryPaths.Add(directoryPath);

      var clangTidyFilePath = Path.Combine(directoryPath, ScriptConstants.kTidyFile);
      if (File.Exists(clangTidyFilePath))
      {
        var settingsPathBuilder = new SettingsPathBuilder();
        var settingsPath = settingsPathBuilder.GetPath("");

        var tempClangTidyFilePath = Path.Combine(settingsPath, ScriptConstants.kTidyFile);
        File.Copy(clangTidyFilePath, tempClangTidyFilePath, true);
        return tempClangTidyFilePath;
      }

      return string.Empty;
    }

    private void ExportJsonCompilationDatabase(string runModeParameters, string genericParameters)
    {
      if (mItemsCollector.IsEmpty)
        return;

      var item = mItemsCollector.Items[0];

      if (item is CurrentSolution)
        Script = JoinUtility.Join(" ", runModeParameters.Remove(runModeParameters.Length - 1), genericParameters, "'");
      else if (item is CurrentProject)
      {
        var itemRelatedParameters = ScriptGenerator.GetItemRelatedParameters(item, true);
        Script = JoinUtility.Join(" ", runModeParameters.Remove(runModeParameters.Length - 1), itemRelatedParameters, genericParameters, "'");
      }
      else if (item is CurrentProjectItem)
      {
        var itemRelatedParameters = mItemsCollector.Items.Count == 1 ?
          ScriptGenerator.GetItemRelatedParameters(item, true) : ScriptGenerator.GetItemRelatedParameters(mItemsCollector.Items, true);

        Script = JoinUtility.Join(" ", runModeParameters.Remove(runModeParameters.Length - 1), itemRelatedParameters, genericParameters, "'");
      }

      PowerShellWrapper.Invoke(Script, runningProcesses);
      OnDataStreamClose(new CloseDataStreamingEventArgs(false));
    }

    #endregion

  }
}
