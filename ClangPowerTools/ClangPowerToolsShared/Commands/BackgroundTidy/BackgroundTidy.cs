using ClangPowerTools.CMake;
using ClangPowerTools.Helpers;
using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;

namespace ClangPowerTools.Commands.BackgroundTidy
{
  public class BackgroundTidy : IDisposable
  {
    #region Members

    private readonly DataProcessor dataProcessor = new DataProcessor();
    private readonly PowerShellWrapperBackground powerShell = new PowerShellWrapperBackground();

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

    #endregion


    #region Constructor

    public BackgroundTidy()
    {
      powerShell.DataHandler += dataProcessor.ReceiveData;
      powerShell.DataErrorHandler += dataProcessor.ReceiveData;
      powerShell.ExitedHandler += dataProcessor.ClosedDataConnection;
    }

    public void Dispose()
    {
      powerShell.DataHandler -= dataProcessor.ReceiveData;
      powerShell.DataErrorHandler -= dataProcessor.ReceiveData;
      powerShell.ExitedHandler -= dataProcessor.ClosedDataConnection;
    }

    #endregion

    public void Run(Document document, int commandId)
    {
      try
      {
        #region Create currnet project item 

        if (document == null || document.ProjectItem == null)
          return;

        var projectName = document.ProjectItem.ContainingProject.FullName;
        if (string.IsNullOrWhiteSpace(projectName))
          return;

        IItem item = new CurrentProjectItem(document.ProjectItem);

        #endregion


        #region Get VS edition and version

        var dte = (DTE2)VsServiceProvider.GetService(typeof(DTE2));

        var vsEdition = dte.Edition;
        mVsVersions.TryGetValue(dte.Version, out string vsVersion);

        #endregion


        #region Generate powershell script

        string runModeParameters = ScriptGenerator.GetRunModeParamaters();
        string genericParameters = ScriptGenerator.GetGenericParamaters(commandId, vsEdition, vsVersion, false);

        CMakeBuilder cMakeBuilder = new CMakeBuilder();
        if (SolutionInfo.IsOpenFolderModeActive())
        {
          cMakeBuilder.Build();
        }

        var vsSolution = SolutionInfo.IsOpenFolderModeActive() == false ?
          (IVsSolution)VsServiceProvider.GetService(typeof(SVsSolution)) : null;

        var itemRelatedParameters = ScriptGenerator.GetItemRelatedParameters(item);

        var psScript = JoinUtility.Join(" ", runModeParameters.Remove(runModeParameters.Length - 1), itemRelatedParameters, genericParameters, "'");

        #endregion


        #region PowerShell Invocation

        powerShell.Invoke(psScript, new RunningProcesses(true));

        #endregion

        if (SolutionInfo.IsOpenFolderModeActive())
        {
          cMakeBuilder.ClearBuildCashe();
        }
      }
      catch (Exception)
      {
        //MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }

    }
  }
}
