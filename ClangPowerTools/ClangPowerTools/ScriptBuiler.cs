using EnvDTE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.VCProjectEngine;
using System.Text.RegularExpressions;

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
      string[] includeDirectoriesOfProject = new string[] {};
      if (aItem is SelectedProjectItem)
      {
        ProjectItem projectItem = aItem.GetObject() as ProjectItem;
        parentDirectoryPath = new DirectoryInfo(projectItem.ContainingProject.FullName).Parent.FullName;
        string containingProject = projectItem.ContainingProject.FullName;
        string containingProjectName = containingProject.Substring(containingProject.LastIndexOf('\\') + 1);
        script = $"{script} {ScriptConstants.kProject} {containingProjectName} {ScriptConstants.kFile} {aFileName}";
        includeDirectoriesOfProject = GetIncludeDirectoriesFromProject(projectItem.ContainingProject);
      }
      else if (aItem is SelectedProject)
      {
        Project project = aItem.GetObject() as Project;
        parentDirectoryPath = new DirectoryInfo(project.FullName).Parent.FullName;
        script = $"{script} {ScriptConstants.kProject} {aFileName}";
        includeDirectoriesOfProject = GetIncludeDirectoriesFromProject(project);
      }
      // Combine include directories from project files and clang power tools configuration. 
      // @todo: This look ugly. We should look at the overall design for a smarter solution
      if (0 < includeDirectoriesOfProject.Length)
      {
        if (mParameters.Contains(ScriptConstants.kIncludeDirectores))
        {
          string pattern = $@"{ScriptConstants.kIncludeDirectores} \((.*)\)";
          string replacement = $" {ScriptConstants.kIncludeDirectores} ($1, ''{String.Join("'',''", includeDirectoriesOfProject)}'')";
          Regex rgx = new Regex(pattern);
          string result = rgx.Replace(mParameters, replacement);
          return $"{script} {result} {ScriptConstants.kDirectory} ''{parentDirectoryPath}'' {ScriptConstants.kLiteral}'";
        }
        else
        {
          string includeDirectoriesOfProjectParameter = $" {ScriptConstants.kIncludeDirectores} (''{String.Join("'',''", includeDirectoriesOfProject)}'')";
          return $"{script} {mParameters} {includeDirectoriesOfProjectParameter} {ScriptConstants.kDirectory} ''{parentDirectoryPath}'' {ScriptConstants.kLiteral}'";
        }
      }
      return $"{script} {mParameters} {ScriptConstants.kDirectory} ''{parentDirectoryPath}'' {ScriptConstants.kLiteral}'";
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

    private string[] GetIncludeDirectoriesFromProject(EnvDTE.Project project)
    {
      List<string> includeDirectories = new List<string>();
      VCProject vcproject = (VCProject)project.Object;

      IVCCollection configurationsCollection = (IVCCollection)vcproject.Configurations;

      Configuration dteActiveConfiguration = project.ConfigurationManager.ActiveConfiguration;

      VCConfiguration vcActiveConfiguration = null;
      foreach (VCConfiguration configuration in configurationsCollection)
      {
        if (configuration.ConfigurationName == dteActiveConfiguration.ConfigurationName && configuration.Platform.Name == dteActiveConfiguration.PlatformName)
        {
          vcActiveConfiguration = configuration;
          break;
        }
      }

      IVCCollection toolsCollection = (IVCCollection)vcActiveConfiguration.Tools;

      foreach (Object toolObject in toolsCollection)
      {
        if (toolObject is VCCLCompilerTool)
        {
          VCCLCompilerTool compilerTool = (VCCLCompilerTool)toolObject;
          string additionalIncludeDirectories = compilerTool.AdditionalIncludeDirectories;
          includeDirectories.AddRange( additionalIncludeDirectories.Split(';') );
          break;
        }
      }

      includeDirectories.AddRange( AdditionalIncludeDirectoriesFromAllPropertySheets(vcActiveConfiguration.PropertySheets) );

      List<string> evaluatedIncludeDirectories = new List<string>();
      foreach (string includeDirectory in includeDirectories)
      {
        string evaluatedIncludeDirectory = vcActiveConfiguration.Evaluate(includeDirectory);
        if (evaluatedIncludeDirectory != "")
        {
          evaluatedIncludeDirectories.Add(evaluatedIncludeDirectory);
        }
      }
      return evaluatedIncludeDirectories.ToArray();
    }

    private static List<string> AdditionalIncludeDirectoriesFromAllPropertySheets(IVCCollection vcActiveConfiguration)
    {
      List<string> additionalIncludes = new List<string>();
      foreach (VCPropertySheet propertySheet in vcActiveConfiguration)
      {
        VCCLCompilerTool compilerToolPropertySheet = (VCCLCompilerTool)((IVCCollection)propertySheet.Tools).Item("VCCLCompilerTool");

        if (compilerToolPropertySheet != null)
        {
          additionalIncludes.AddRange(compilerToolPropertySheet.AdditionalIncludeDirectories.Split(';'));
          IVCCollection InherPSS = propertySheet.PropertySheets;
          if (InherPSS != null)
          {
            if (InherPSS.Count != 0)
            {
              additionalIncludes.AddRange(AdditionalIncludeDirectoriesFromAllPropertySheets(InherPSS));
            }
          }
        }
      }
      return additionalIncludes;
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

      if (null != aGeneralOptions.IncludeDirectories && 0 < aGeneralOptions.IncludeDirectories.Length)
        parameters = $"{parameters} {ScriptConstants.kIncludeDirectores} (''{String.Join("'',''", aGeneralOptions.IncludeDirectories)}'')";

      if (null != aGeneralOptions.ProjectsToIgnore && 0 < aGeneralOptions.ProjectsToIgnore.Length)
        parameters = $"{parameters} {ScriptConstants.kProjectsToIgnore} (''{String.Join("'',''", aGeneralOptions.ProjectsToIgnore)}'')";

      if (null != aGeneralOptions.FilesToIgnore && 0 < aGeneralOptions.FilesToIgnore.Length)
        parameters = $"{parameters} {ScriptConstants.kFilesToIgnore} (''{String.Join("'',''", aGeneralOptions.FilesToIgnore)}'')";

      return $"{parameters}";
    }

    private string GetTidyParameters(TidyOptions aTidyPage)
    {
      string parameters = aTidyPage.Fix ? $" {ScriptConstants.kTidyFix} ''-*," : $" {ScriptConstants.kTidy} ''-*,";
     
      if (null != aTidyPage.TidyChecks && 0 < aTidyPage.TidyChecks.Length)
        parameters = $"{parameters}{String.Join(",", aTidyPage.TidyChecks)}''";
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
