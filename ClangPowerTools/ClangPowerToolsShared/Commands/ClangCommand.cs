using ClangPowerTools.CMake;
using ClangPowerTools.Commands;
using ClangPowerTools.Events;
using ClangPowerTools.Helpers;
using ClangPowerTools.IgnoreActions;
using ClangPowerTools.Services;
using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.Commands.Models;
using ClangPowerToolsShared.Helpers;
using ClangPowerToolsShared.MVVM.Constants;
using ClangPowerToolsShared.MVVM.Views.ToolWindows;
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

    //public static RunningProcesses runningProcesses = new RunningProcesses();

    private CacheProjectsItemsModel cacheProjectsItemsModel = new CacheProjectsItemsModel();

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
    public event EventHandler<ActiveDocumentEventArgs> ActiveDocumentEvent;

    public event EventHandler<ClangCommandMessageEventArgs> IgnoredItemsEvent;
    protected static object mutex = new object();

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

    protected void CacheProjectsFromItems()
    {
      cacheProjectsItemsModel.Projects.Clear();
      cacheProjectsItemsModel.ProjectItems.Clear();

      foreach (var item in mItemsCollector.Items)
      {
        if (item.GetObject() is Project)
        {
          var project = item.GetObject() as Project;
          cacheProjectsItemsModel.Projects.Add(project);
        }
        else if (item.GetObject() is ProjectItem)
        {
          var projectItem = item.GetObject() as ProjectItem;
          cacheProjectsItemsModel.ProjectItems.Add(projectItem);
        }
      }
    }

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
    protected void CollectItemsDependingOnCommandLocation(
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
        case CommandUILocation.ViewMenu:
          mItemsCollector.CollectProjectItems();
          break;
      }
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

    protected void OnIgnoreItem(ClangCommandMessageEventArgs e)
    {
      IgnoredItemsEvent?.Invoke(this, e);
    }

    #endregion

    #region Private Methods

    protected void InvokeFindCommand(FindToolWindow findToolWindow)
    {
      if (findToolWindow != null)
        findToolWindow.RunQuery();

      if (RunController.StopCommandActivated)
      {
        RunController.OnDataStreamClose(new CloseDataStreamingEventArgs(true));
        RunController.StopCommandActivated = false;
      }
      else
      {
        RunController.OnDataStreamClose(new CloseDataStreamingEventArgs(false));
      }
    }

    /// <summary>
    /// Create a process for running clang-doc.exe resulted
    /// format, depends on passed command
    /// </summary>
    /// <param name="commandId"></param>
    /// <param name="jsonCompilationDbActive"></param>
    /// <param name="paths"></param>
    /// <returns></returns>
    protected void GenerateDocumentationForProject(int commandId, AsyncPackage package)
    {
      
      var jsonCompilationDatabasePath = PathConstants.JsonCompilationDBPath;
      string documentationOutoutePath = GenerateDocumentation.FindOutputFolderName(
        Path.Combine(new FileInfo(jsonCompilationDatabasePath).Directory.FullName,
        "Documentation\\"));
      string clangDocPath = PowerShellWrapper.DownloadTool(ScriptConstants.kClangDoc);
      clangDocPath = Path.Combine(clangDocPath, ScriptConstants.kClangDoc);

      if (File.Exists(jsonCompilationDatabasePath) && File.Exists(clangDocPath))
      {
        string projectArguments = GetProjectName() == string.Empty ? string.Empty : $"--project-name='{GetProjectName()}'";

        GenerateDocumentation.OutputDir = documentationOutoutePath;
        CommandControllerInstance.CommandController.DisplayMessage(false, "Please wait ...");
        string Script = $"-ExecutionPolicy Unrestricted -NoProfile -Noninteractive -command \"& " +
        $"'{clangDocPath}' --public {projectArguments} --format={GenerateDocumentation.Formats[commandId]}  " +
        $"-output='{documentationOutoutePath}' '{jsonCompilationDatabasePath}' \"";

        PowerShellWrapper.Invoke(Script);

        //Replace a string in index_json.js if is generated with html format, to avoid a json error
        string indexJsonFileName = Path.Combine(documentationOutoutePath, "index_json.js");
        if (File.Exists(indexJsonFileName) && commandId == CommandIds.kDocumentationHtmlId)
        {
          string indexJsonFileContent = File.ReadAllText(indexJsonFileName);
          indexJsonFileContent = indexJsonFileContent.Replace("var JsonIndex = `", "var JsonIndex = String.raw `");
          File.WriteAllText(indexJsonFileName, indexJsonFileContent);
        }

        DeleteJsonCompilationDB();

        if (RunController.StopCommandActivated)
        {
          RunController.OnDataStreamClose(new CloseDataStreamingEventArgs(true));
          RunController.StopCommandActivated = false;
        }
        else
        {
          RunController.OnDataStreamClose(new CloseDataStreamingEventArgs(false));
        }
      }
    }

    private void DeleteJsonCompilationDB()
    {
      var jsonCompilationDatabasePath = PathConstants.JsonCompilationDBPath;
      if (File.Exists(jsonCompilationDatabasePath))
        File.Delete(jsonCompilationDatabasePath);
    }

    private void Compile(string runModeParameters, string genericParameters, int commandId, List<string> paths)
    {
      var vsSolution = SolutionInfo.IsOpenFolderModeActive() == false ?
        (IVsSolution)VsServiceProvider.GetService(typeof(SVsSolution)) : null;

      try
      {
        var tempClangTidyFilePath = string.Empty;
        if (paths is not null)
        {
          var itemRelatedParameters = ScriptGenerator.GetItemRelatedParametersCustomPaths(paths, cacheProjectsItemsModel);
          Script = JoinUtility.Join(" ", runModeParameters, itemRelatedParameters, genericParameters, "\"");
        }
        else
        {
          var item = mItemsCollector.Items[0];
          var ignoreItem = new IgnoreItem();

          //TODO Display all ignored files
          //if (ignoreItem.Check(item))
          //{
          //  OnIgnoreItem(new ClangCommandMessageEventArgs(ignoreItem.IgnoreCompileOrTidyMessage, false));
          //}

          //TODO Handle CreateTemporaryFileForTidy
          if (commandId == CommandIds.kTidyId || commandId == CommandIds.kTidyFixId || CommandIds.kTidyToolWindowId == commandId ||
            commandId == CommandIds.kTidyToolbarId || commandId == CommandIds.kTidyFixToolbarId)
          {
            tempClangTidyFilePath = CreateTemporaryFileForTidy(item);
          }

          var itemRelatedParameters = string.Empty;
          if (item is CurrentProject || item is Project || item is Solution)
          {
            itemRelatedParameters = ScriptGenerator.GetProjectRelatedParameters(cacheProjectsItemsModel);
          }
          else if (item is CurrentProjectItem || item is ProjectItem)
          {
            itemRelatedParameters = ScriptGenerator.GetItemRelatedParameters(mItemsCollector.Items);
          }

          // From the first parameter is removed the last character which is mandatory "'"
          // and added to the end of the string to close the script escaping command
          Script = JoinUtility.Join(" ", runModeParameters, itemRelatedParameters, genericParameters, "\"");

          ItemHierarchy = vsSolution != null ? AutomationUtil.GetItemHierarchy(vsSolution, item) : null;
        }
        PowerShellWrapper.Invoke(Script);

        if (RunController.StopCommandActivated)
        {
          RunController.OnDataStreamClose(new CloseDataStreamingEventArgs(true));
          RunController.StopCommandActivated = false;
        }
        else
        {
          if (File.Exists(tempClangTidyFilePath))
          {
            File.Delete(tempClangTidyFilePath);
          }
          RunController.OnDataStreamClose(new CloseDataStreamingEventArgs(false));
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
        Script = JoinUtility.Join(" ", runModeParameters, genericParameters, "\"");
      else if (item is CurrentProject)
      {
        var itemRelatedParameters = ScriptGenerator.GetItemRelatedParameters(item, true);
        Script = JoinUtility.Join(" ", runModeParameters, itemRelatedParameters, genericParameters, "\"");
      }
      else if (item is CurrentProjectItem)
      {
        var itemRelatedParameters = mItemsCollector.Items.Count == 1 ?
          ScriptGenerator.GetItemRelatedParameters(item, true) : ScriptGenerator.GetItemRelatedParameters(mItemsCollector.Items, true);

        Script = JoinUtility.Join(" ", runModeParameters, itemRelatedParameters, genericParameters, "\"");
      }

      PowerShellWrapper.Invoke(Script);
      RunController.OnDataStreamClose(new CloseDataStreamingEventArgs(false));
    }

    private string GetProjectName()
    {
      string projectName = string.Empty;
      try
      {
        if (cacheProjectsItemsModel.Projects.Count > 0)
        {
          projectName = cacheProjectsItemsModel.Projects[0].FullName;
        }
        else
        {
          projectName = cacheProjectsItemsModel.ProjectItems[0].ContainingProject.FullName;
        }
      }
      catch (NullReferenceException e)
      {

      }
      return projectName;
    }

    #endregion

  }
}
