using ClangPowerTools.CMake;
using ClangPowerTools.Helpers;
using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClangPowerTools.Commands.BackgroundClangTidy
{
  public class BackgroundClangTidy
  {

    private readonly Dictionary<string, string> mVsVersions = new Dictionary<string, string>
    {
      {"11.0", "2010"},
      {"12.0", "2012"},
      {"13.0", "2013"},
      {"14.0", "2015"},
      {"15.0", "2017"},
      {"16.0", "2019"}
    };

    public async Task RunAsync(Document document, int commandId)
    {
      try
      {
        #region Create currnet project item 

        if (document == null)
          return;

        var projectName = document.ProjectItem.ContainingProject.FullName;
        if (string.IsNullOrWhiteSpace(projectName))
          return;

        IItem item = new CurrentProjectItem(document.ProjectItem);

        #endregion


        #region Get VS edition and version

        var dte = (DTE2)VsServiceProvider.GetService(typeof(DTE));

        var vsEdition = dte.Edition;
        mVsVersions.TryGetValue(dte.Version, out string vsVersion);

        #endregion


        #region Generate powershell script

        string runModeParameters = ScriptGenerator.GetRunModeParamaters();
        string genericParameters = ScriptGenerator.GetGenericParamaters(commandId, vsEdition, vsVersion);

        CMakeBuilder cMakeBuilder = new CMakeBuilder();
        cMakeBuilder.Build();

        var vsSolution = SolutionInfo.IsOpenFolderModeActive() == false ?
          (IVsSolution)VsServiceProvider.GetService(typeof(SVsSolution)) : null;

        var itemRelatedParameters = ScriptGenerator.GetItemRelatedParameters(item);

        var psScript = JoinUtility.Join(" ", runModeParameters.Remove(runModeParameters.Length - 1), itemRelatedParameters, genericParameters, "'");

        #endregion


        #region PowerShell Invocation

        DataProcessor dataProcessor = new DataProcessor();
        PowerShellWrapperBackground powerShell = new PowerShellWrapperBackground();

        powerShell.DataHandler += dataProcessor.ReceiveData;
        powerShell.DataErrorHandler += dataProcessor.ReceiveData;
        powerShell.ExitedHandler += dataProcessor.ClosedDataConnection;

        //TODO -> store de process in such a way that it can offer access to the stop command

        //powerShell.Invoke(psScript, );


        powerShell.DataHandler -= dataProcessor.ReceiveData;
        powerShell.DataErrorHandler -= dataProcessor.ReceiveData;
        powerShell.ExitedHandler -= dataProcessor.ClosedDataConnection;

        #endregion
















        cMakeBuilder.ClearBuildCashe();

      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }



    }
  }
}
