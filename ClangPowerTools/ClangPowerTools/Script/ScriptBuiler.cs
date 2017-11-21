using EnvDTE;
using EnvDTE80;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ClangPowerTools
{
  public class ScriptBuiler
  {
    #region Members

    private string mParameters = string.Empty;

    #endregion

    #region Public Methods

    public string GetScript(IItem aItem, string aFileName)
    {
      string containingDirectoryPath = string.Empty;
      string parentDirectoryPath = string.Empty;
      string script = $"{ScriptConstants.kScriptBeginning} ''{GetScriptPath()}''";

      if (aItem is SelectedProjectItem)
      {
        ProjectItem projectItem = aItem.GetObject() as ProjectItem;
        parentDirectoryPath = new DirectoryInfo(projectItem.ContainingProject.FullName).Parent.FullName;
        string containingProject = projectItem.ContainingProject.FullName;
        string containingProjectName = containingProject.Substring(containingProject.LastIndexOf('\\') + 1);
        script = $"{script} {ScriptConstants.kProject} {containingProjectName} {ScriptConstants.kFile} {aFileName} " +
          $"{ScriptConstants.kActiveConfiguration} ''{ProjectConfiguration.GetConfiguration(projectItem.ContainingProject)}|{ProjectConfiguration.GetPlatform(projectItem.ContainingProject)}''";
      }
      else if (aItem is SelectedProject)
      {
        Project project = aItem.GetObject() as Project;
        parentDirectoryPath = new DirectoryInfo(project.FullName).Parent.FullName;
        script = $"{script} {ScriptConstants.kProject} {aFileName} ''{ProjectConfiguration.GetConfiguration(project)}|{ProjectConfiguration.GetPlatform(project)}''";
      }
      return $"{script} {mParameters} {ScriptConstants.kDirectory} ''{parentDirectoryPath}'' {ScriptConstants.kLiteral}'";
    }

    public void ConstructParameters(GeneralOptions aGeneralOptions, TidyOptions aTidyOptions, 
      TidyChecks aTidyChecks, DTE2 aDte, string aVsEdition, string aVsVersion)
    {
      mParameters = GetGeneralParameters(aGeneralOptions);
      mParameters = null != aTidyOptions ?
        $"{mParameters} {GetTidyParameters(aTidyOptions, aTidyChecks)}" : $"{mParameters} {ScriptConstants.kParallel}";
      mParameters = $"{mParameters} {ScriptConstants.kVsVersion} {aVsVersion} {ScriptConstants.kVsEdition} {aVsEdition}";
    }

    #endregion

    #region Get Parameters Helpers

    private string GetScriptPath()
    {
      string assemblyPath = Assembly.GetExecutingAssembly().Location;
      assemblyPath = assemblyPath.Substring(0, assemblyPath.LastIndexOf('\\'));
      return $"{assemblyPath}\\{ScriptConstants.kScriptName}";
    }

    private string GetGeneralParameters(GeneralOptions aGeneralOptions)
    {
      string parameters = string.Empty;

      if (null != aGeneralOptions.ClangFlags && 0 < aGeneralOptions.ClangFlags.Length)
      {
        parameters = $"{parameters} {ScriptConstants.kClangFlags} (" +
          $"{string.Format("{0}", aGeneralOptions.TreatWarningsAsErrors ? string.Format("''{0}'',",ScriptConstants.kTreatWarningsAsErrors) : string.Empty)}''" +
          $"{String.Join("'',''", aGeneralOptions.ClangFlags)}'')";
      }

      if (aGeneralOptions.Continue)
        parameters = $"{parameters} {ScriptConstants.kContinue}";

      if(aGeneralOptions.VerboseMode) 
        parameters = $"{parameters} {ScriptConstants.kVerboseMode}";

      if (null != aGeneralOptions.ProjectsToIgnore && 0 < aGeneralOptions.ProjectsToIgnore.Length)
        parameters = $"{parameters} {ScriptConstants.kProjectsToIgnore} (''{String.Join("'',''", aGeneralOptions.ProjectsToIgnore)}'')";

      if (null != aGeneralOptions.FilesToIgnore && 0 < aGeneralOptions.FilesToIgnore.Length)
        parameters = $"{parameters} {ScriptConstants.kFilesToIgnore} (''{String.Join("'',''", aGeneralOptions.FilesToIgnore)}'')";

      return $"{parameters}";
    }

    private string GetTidyParameters(TidyOptions aTidyOptions, TidyChecks aTidyChecks)
    {
      string parameters = string.Empty;
      if (null != aTidyOptions.TidyChecks && 0 < aTidyOptions.TidyChecks.Length)
        parameters = $",{String.Join(",", aTidyOptions.TidyChecks)}";
      else
      {
        foreach (PropertyInfo prop in aTidyChecks.GetType().GetProperties())
        {
          object[] propAttrs = prop.GetCustomAttributes(false);
          object clangCheckAttr = propAttrs.FirstOrDefault(attr => typeof(ClangCheckAttribute) == attr.GetType());
          object displayNameAttrObj = propAttrs.FirstOrDefault(attr => typeof(DisplayNameAttribute) == attr.GetType());

          if ( null == clangCheckAttr || null == displayNameAttrObj)
            continue;

          DisplayNameAttribute displayNameAttr = (DisplayNameAttribute)displayNameAttrObj;
          var value = prop.GetValue(aTidyChecks, null);
          if (Boolean.TrueString != value.ToString())
            continue;
          parameters = $"{parameters},{displayNameAttr.DisplayName}";
        }
      }
      if (string.Empty != parameters)
        parameters = string.Format("{0} ''-*{1}''", 
          aTidyOptions.Fix ? ScriptConstants.kTidyFix : ScriptConstants.kTidy, parameters);

      return parameters;
    }

    #endregion

  }
}
