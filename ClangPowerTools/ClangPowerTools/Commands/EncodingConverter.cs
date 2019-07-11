﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using ClangPowerTools.MVVM.Helpers;
using ClangPowerTools.MVVM.ViewModels;
using ClangPowerTools.Properties;
using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ClangPowerTools.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class EncodingConverter : BasicCommand
  {
    #region Properties

    public static EncodingConverter Instance
    {
      get;
      private set;
    }

    #endregion

    #region Constructor
    private EncodingConverter(CommandController commandController, AsyncPackage package, OleMenuCommandService commandService, Guid guid, int id)
         : base(package, guid, id)
    {
      if (commandService != null)
      {
        var menuCommandID = new CommandID(CommandSet, Id);
        var menuCommand = new OleMenuCommand(commandController.Execute, menuCommandID);
        menuCommand.BeforeQueryStatus += commandController.OnBeforeClangCommand;
        menuCommand.Enabled = true;
        commandService.AddCommand(menuCommand);
      }
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(CommandController commandController, AsyncPackage package, Guid guid, int id)
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

      OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
      Instance = new EncodingConverter(commandController, package, commandService, guid, id);
    }


    public async void RunEncodingConverter(int id, CommandUILocation commandUILocation)
    {
      var selectedFiles = GetSelectedFile(commandUILocation);
      var dte = VsServiceProvider.GetService(typeof(DTE)) as DTE2;
      Array selectedItems = dte.ToolWindows.SolutionExplorer.SelectedItems as Array;
      foreach (UIHierarchyItem item in selectedItems)
      {
        if (item.Object is Solution)
        {
          selectedFiles.AddRange(GetAllFilesWithExtension(Path.GetDirectoryName(dte.Solution.FullName), "*.sln"));
          selectedFiles.AddRange(GetAllFilesWithExtension(Path.GetDirectoryName(dte.Solution.FullName), "*.vcxproj"));
        }
        else if (item.Object is Project)
        {
          //ProjectItem prjItem = item.Object as ProjectItem;
          //string filePath = prjItem.Properties.Item("FullPath").Value.ToString();
          //selectedFiles.AddRange(GetAllFilesWithExtension(Path.GetDirectoryName(filePath), "*.vcxproj"));
        }
      }

      //if (solution)
      // ChangeEncodingForSolution
      // ChangeEncodingForProject(GetSolutionPath())
      // ChangeEncodingForFiles()

      // if(projects)
      // ChangeEncodingForProject(GetSolutionPath())
      // ChangeEncodingForFiles()


      // GetSolutionPath()
      // return the solution path

      // ChangeEncodingForSolution
      // UTF-8 solution .sln file

      // ChangeEncodingForProject()
      // UTF-8 projets vcxproj file

      // ChangeEncodingForFiles()
      // UTF-8 files

      if (selectedFiles == null)
      {
        return;
      }
      var encodingConverterViewModel = new EncodingConverterViewModel(selectedFiles);
      await encodingConverterViewModel.LoadData();

      var EncodingConverterWindow = WindowManager.CreateElementWindow(encodingConverterViewModel, Resources.EncodingConverterWindowTitle, "ClangPowerTools.MVVM.Views.EncodingConverterControl");

      if (encodingConverterViewModel.CloseAction == null)
      {
        encodingConverterViewModel.CloseAction = () => EncodingConverterWindow.Close();
      }

      EncodingConverterWindow.ShowDialog();
    }

    private List<string> GetSelectedFile(CommandUILocation commandUILocation)
    {
      if (commandUILocation == CommandUILocation.ContextMenu)
      {
        var itemsCollector = new ItemsCollector(ScriptConstants.kAcceptedFileExtensions);
        itemsCollector.CollectSelectedProjectItems();
        var itemsList = new List<string>();
        foreach (var item in itemsCollector.Items)
        {
          itemsList.Add(item.GetPath());
        }
        return itemsList;
      }
      else if (commandUILocation == CommandUILocation.Toolbar)
      {
        //var itemsCollector = new ItemsCollector(ScriptConstants.kAcceptedFileExtensions);
        //itemsCollector.CollectActiveProjectItem();
        //return itemsCollector.Items;
        return null;
      }
      else
      {
        return null;
      }
    }

    private List<string> GetAllFilesWithExtension(string folderPath, string extensionName )
    {
      List<string> files = new List<string>();
      foreach (string file in Directory.GetFiles(folderPath, extensionName, SearchOption.AllDirectories))
      {
        files.Add(file);
      }
      return files;
    }

    #endregion
  }
}
