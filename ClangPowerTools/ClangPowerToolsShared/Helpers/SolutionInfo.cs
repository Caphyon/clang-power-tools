using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.IO;

namespace ClangPowerTools.Helpers
{
  public static class SolutionInfo
  {
    #region Properties

    public static bool SolutionOpen { get; set; }

    public static bool OpenFolderModeActive { get; set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Returns the required solution file information.
    /// </summary>
    /// <param name="dir">Pointer to the solution directory</param>
    /// <param name="file">Pointer to the solution file name</param>
    /// <param name="ops">Pointer to the solutions options file name</param>
    /// <returns>If the method succeeds, it returns Microsoft.VisualStudio.VSConstants.S_OK. If it fails, it returns an error code.</returns>
    public static int GetSolutionInfo(out string dir, out string file, out string optionFile)
    {
      var solution = (IVsSolution)VsServiceProvider.GetService(typeof(SVsSolution));
      return solution.GetSolutionInfo(out dir, out file, out optionFile);
    }

    /// <summary>
    /// Check if any VS Solution is open
    /// </summary>
    /// <returns>True if any VS Solution is open. False otherwise.</returns>
    public static bool IsSolutionOpen()
    {
      var solution = (IVsSolution)VsServiceProvider.GetService(typeof(SVsSolution));
      solution.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object open);
      SolutionOpen = (bool)open;
      return SolutionOpen;
    }

    /// <summary>
    /// Check if VS runs in Open Folder Mode
    /// </summary>
    /// <returns>True if VS runs in Open Folder Mode. False otherwise.</returns>
    public static bool IsOpenFolderModeActive()
    {
      var solution = (IVsSolution)VsServiceProvider.GetService(typeof(SVsSolution));
      solution.GetProperty((int)__VSPROPID7.VSPROPID_IsInOpenFolderMode, out object folderMode);

      OpenFolderModeActive = folderMode == null ? false : (bool)folderMode;
      return OpenFolderModeActive;
    }

    public static bool ContainsCppProject()
    {
      DTE2 dte = (DTE2)VsServiceProvider.GetService(typeof(DTE2));
      Solution solution = dte.Solution;

      if (solution == null)
      {
        return false;
      }

      return AnyCppProject(solution);
    }

    public static bool AnyCppProject(Solution solution)
    {
      foreach (var project in solution)
      {
        if (IsCppProject((Project)project))
        {
          return true;
        }
      }
      return false;
    }

    public static bool IsCppProject(Project project)
    {
      return project.Kind.Equals(ScriptConstants.kCppProjectGuid);
    }

    public static bool AreContextMenuCommandsEnabled()
    {
      if (IsOpenFolderModeActive())
      {
        return true;
      }

      ItemsCollector itemCollector = new ItemsCollector();
      itemCollector.CollectSelectedItems();
      List<string> selectedItems = new List<string>();
      if (itemCollector.IsEmpty)
        return false;

      itemCollector.Items.ForEach(e => selectedItems.Add(e.GetName()));

      if (selectedItems.Count == 0)
      {
        return false;
      }

      foreach (var item in selectedItems)
      {
        var fileExtension = Path.GetExtension(item).ToLower();
        if (ScriptConstants.kAcceptedFileExtensions.Contains(fileExtension))
        {
          return true;
        }
      }

      return false;
    }

    public static bool AreToolbarCommandsEnabled()
    {
      if (IsOpenFolderModeActive())
        return true;

      ItemsCollector itemCollector = new ItemsCollector();
      itemCollector.CollectActiveProjectItem();

      if (itemCollector.IsEmpty)
        return false;

      string activeItem = itemCollector.Items[0]?.GetName().ToLower();
      var fileExtension = Path.GetExtension(activeItem);
      return ScriptConstants.kAcceptedFileExtensionsWithoutHeaders.Contains(fileExtension);
    }

    public static bool ActiveDocumentValidation()
    {
      var document = DocumentHandler.GetActiveDocument();
      if (document == null || string.IsNullOrWhiteSpace(document.FullName))
        return false;

      var extensionDocument = Path.GetExtension(document.FullName);
      return ScriptConstants.kExtendedAcceptedFileExtensions.Contains(extensionDocument);
    }

    #endregion

  }
}
