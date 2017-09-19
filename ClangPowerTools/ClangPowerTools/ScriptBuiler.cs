using EnvDTE;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

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
        aFileName = aFileName.Substring(0, aFileName.IndexOf('.'));
        ProjectItem projectItem = aItem.GetObject() as ProjectItem;
        parentDirectoryPath = new DirectoryInfo(projectItem.ContainingProject.FullName).Parent.FullName;
        script = $"{script} {ScriptConstants.kProject} {projectItem.ContainingProject.Name} {ScriptConstants.kFile} {aFileName}";
      }
      else if (aItem is SelectedProject)
      {
        Project project = aItem.GetObject() as Project;
        parentDirectoryPath = new DirectoryInfo(project.FullName).Parent.FullName;
        script = $"{script} {ScriptConstants.kProject} {aFileName}";
      }
      return $"{script} {mParameters} {ScriptConstants.kDirectory} ''{parentDirectoryPath}'''";
    }

    public void ConstructParameters(GeneralOptions aGeneralOptions, 
      TidyOptions aTidyPage, string aVsEdition, string aVsVersion)
    {
      mParameters = GetGeneralParameters(aGeneralOptions);
      mParameters = null != aTidyPage ?
        $"{mParameters} {GetTidyParameters(aTidyPage)}" : $"{mParameters} {ScriptConstants.kParallel}";
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
        parameters = $"{parameters} {ScriptConstants.kClangFlags} (''{String.Join("'',''", aGeneralOptions.ClangFlags)}'')";
      
      if (aGeneralOptions.Continue)
        parameters = $"{parameters} {ScriptConstants.kContinue}";
      
      if (null != aGeneralOptions.IncludeDirectories && 0 < aGeneralOptions.IncludeDirectories.Length)
        parameters = $"{parameters} {ScriptConstants.kIncludeDirectores} {String.Join(",", aGeneralOptions.IncludeDirectories)}";
      
      if (null != aGeneralOptions.ProjectsToIgnore && 0 < aGeneralOptions.ProjectsToIgnore.Length)
        parameters = $"{parameters} {ScriptConstants.kProjectsToIgnore} {String.Join(",", aGeneralOptions.ProjectsToIgnore)}";

      return $"{parameters}";
    }

    private string GetTidyParameters(TidyOptions aTidyPage)
    {
      string parameters = aTidyPage.Fix ? $" {ScriptConstants.kTidyFix} ''-*" : $" {ScriptConstants.kTidy} ''-*";
     
      if (null != aTidyPage.TidyChecks && 0 < aTidyPage.TidyChecks.Length)
        parameters = $"{parameters} ''-*,{String.Join(",", aTidyPage.TidyChecks)}''";
      else
      {
        foreach (PropertyInfo prop in aTidyPage.GetType().GetProperties())
        {
          object[] propAttrs = prop.GetCustomAttributes(false);
          object clangCheckAttr = propAttrs.FirstOrDefault(attr => typeof(ClangCheckAttribute) == attr.GetType());
          object displayNameAttrObj = propAttrs.FirstOrDefault(attr => typeof(DisplayNameAttribute) == attr.GetType());

          if ( null == clangCheckAttr || null == displayNameAttrObj)
            continue;

          DisplayNameAttribute displayNameAttr = (DisplayNameAttribute)displayNameAttrObj;
          var value = prop.GetValue(aTidyPage, null);
          if (Boolean.TrueString != value.ToString())
            continue;

          parameters = $"{parameters},{displayNameAttr.DisplayName}";
        }
        parameters = $"{parameters}''";
      }
      return parameters;
    }

    #endregion

  }
}
