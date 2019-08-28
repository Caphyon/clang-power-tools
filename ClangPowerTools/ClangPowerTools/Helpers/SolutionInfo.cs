﻿using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;
using System.Linq;

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
      OpenFolderModeActive = (bool)folderMode;
      return OpenFolderModeActive;
    }

    public static bool ContainsFile(string fileName)
    {

      VsServiceProvider.TryGetService(typeof(DTE), out object dte);
      var solution = (dte as DTE2).Solution;

      if (solution == null || solution.IsOpen == false)
      {
        return false;
      }

      string file = Directory.GetFiles(Path.GetDirectoryName(solution.FullName), fileName, SearchOption.AllDirectories)
                    .FirstOrDefault();

      return file != null;
    }

    #endregion

  }
}
