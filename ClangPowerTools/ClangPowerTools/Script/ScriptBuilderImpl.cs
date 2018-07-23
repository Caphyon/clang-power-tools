using ClangPowerTools.Convertors;
using ClangPowerTools.DialogPages;
using ClangPowerTools.Error;
using EnvDTE;
using EnvDTE80;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ClangPowerTools.Script
{
  public class ScriptBuilderImpl : IBuilder<string>
  {
    #region Members


    private string mParameters = string.Empty;
    private bool mUseTidyFile = false;


    #endregion


    #region IBuilder Implementation


    /// <summary>
    /// Build the script as a string which will be called in power shell to run clang compiler
    /// </summary>
    public void Build()
    {
      if (string.IsNullOrWhiteSpace(mParameters))
        ConstructParameters();

      GetScript();
    }


    /// <summary>
    /// Get the script result
    /// </summary>
    /// <returns>The script as a string which will be called in power shell to run clang compiler</returns>
    public string GetResult()
    {
      throw new NotImplementedException();
    }


    #endregion


    #region Private Methods


    private void ConstructParameters(ClangGeneralOptionsView aGeneralOptions, ClangTidyOptionsView aTidyOptions, ClangTidyPredefinedChecksOptionsView aTidyChecks,
      ClangTidyCustomChecksOptionsView aTidyCustomChecks, ClangFormatOptionsView aClangFormatView, DTE2 aDTEObj, string aVsEdition, string aVsVersion, bool aTidyFixFlag = false)
    {
      mParameters = GetGeneralParameters(aGeneralOptions);
      mParameters = null != aTidyOptions ?
        $"{mParameters} {GetTidyParameters(aTidyOptions, aTidyChecks, aTidyCustomChecks, aTidyFixFlag)}" : $"{mParameters} {ScriptConstants.kParallel}";

      if (null != aClangFormatView && null != aTidyOptions && true == aTidyFixFlag && true == aTidyOptions.FormatAfterTidy)
        mParameters = $"{mParameters} {ScriptConstants.kClangFormatStyle} {aClangFormatView.Style}";

      mParameters = $"{mParameters} {ScriptConstants.kVsVersion} {aVsVersion} {ScriptConstants.kVsEdition} {aVsEdition}";
    }


    private string GetScript(IItem aItem, string aSolutionPath)
    {
      string containingDirectoryPath = string.Empty;
      string script = $"{ScriptConstants.kScriptBeginning} ''{GetFilePath()}''";

      if (aItem is SelectedProjectItem)
      {
        ProjectItem projectItem = aItem.GetObject() as ProjectItem;
        string containingProject = projectItem.ContainingProject.FullName;
        script = $"{script} {ScriptConstants.kProject} ''{containingProject}'' " +
          $"{ScriptConstants.kFile} ''{projectItem.Properties.Item("FullPath").Value}'' {ScriptConstants.kActiveConfiguration} " +
          $"''{ProjectConfigurationHandler.GetConfiguration(projectItem.ContainingProject)}|{ProjectConfigurationHandler.GetPlatform(projectItem.ContainingProject)}''";
      }
      else if (aItem is SelectedProject)
      {
        Project project = aItem.GetObject() as Project;
        script = $"{script} {ScriptConstants.kProject} ''{project.FullName}'' {ScriptConstants.kActiveConfiguration} " +
          $"''{ProjectConfigurationHandler.GetConfiguration(project)}|{ProjectConfigurationHandler.GetPlatform(project)}''";
      }
      return $"{script} {mParameters} {ScriptConstants.kDirectory} ''{aSolutionPath}'''";
    }


    //Get the script file path
    private string GetFilePath()
    {
      return Path.Combine(GetFolderPath(), ScriptConstants.kScriptName);
    }


    //return the path of the folder where is located the script file or clang-format.exe
    private string GetFolderPath()
    {
      string assemblyPath = Assembly.GetExecutingAssembly().Location;
      return assemblyPath.Substring(0, assemblyPath.LastIndexOf('\\'));
    }


    //Rebuild a semicolon separated list to a PowerShell one, removing empty arguments to prevent errors when starting the script
    private string RebuildParameterList(string list) => string.Join("'',''", list.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

    private string GetGeneralParameters(ClangGeneralOptionsView aGeneralOptions)
    {
      string parameters = string.Empty;

      if (false == string.IsNullOrWhiteSpace(aGeneralOptions.ClangFlags))
      {
        parameters = ScriptConstants.kClangFlags;
        if (true == aGeneralOptions.TreatWarningsAsErrors)
          parameters += string.Format(" (''{0}'',''{1}'')", ScriptConstants.kTreatWarningsAsErrors, RebuildParameterList(aGeneralOptions.ClangFlags));
        else
          parameters += string.Format(" (''{0}'')", RebuildParameterList(aGeneralOptions.ClangFlags));
      }

      if (true == aGeneralOptions.Continue)
        parameters = $"{parameters} {ScriptConstants.kContinue}";

      if (true == aGeneralOptions.VerboseMode)
        parameters = $"{parameters} {ScriptConstants.kVerboseMode}";

      if (false == string.IsNullOrWhiteSpace(aGeneralOptions.ProjectsToIgnore))
        parameters = $"{parameters} {ScriptConstants.kProjectsToIgnore} (''{RebuildParameterList(aGeneralOptions.ProjectsToIgnore)}'')";

      if (false == string.IsNullOrWhiteSpace(aGeneralOptions.FilesToIgnore))
        parameters = $"{parameters} {ScriptConstants.kFilesToIgnore} (''{RebuildParameterList(aGeneralOptions.FilesToIgnore)}'')";

      if (0 == string.Compare(ClangGeneralAdditionalIncludesConvertor.ToString(aGeneralOptions.AdditionalIncludes), ComboBoxConstants.kSystemIncludeDirectories))
        parameters = $"{parameters} {ScriptConstants.kSystemIncludeDirectories}";

      return $"{parameters}";
    }


    private string GetTidyParameters(ClangTidyOptionsView aTidyOptions, ClangTidyPredefinedChecksOptionsView aTidyChecks,
      ClangTidyCustomChecksOptionsView aTidyCustomChecks, bool aTidyFixFlag)
    {
      string parameters = string.Empty;

      if (0 == string.Compare(ComboBoxConstants.kTidyFile, ClangTidyUseChecksFromConvertor.ToString(aTidyOptions.UseChecksFrom)))
      {
        parameters = ScriptConstants.kTidyFile;
        mUseTidyFile = true;
      }
      else if (0 == string.Compare(ComboBoxConstants.kCustomChecks, ClangTidyUseChecksFromConvertor.ToString(aTidyOptions.UseChecksFrom)))
      {
        if (false == string.IsNullOrWhiteSpace(aTidyCustomChecks.TidyChecks))
          parameters = $",{aTidyCustomChecks.TidyChecks.Replace(';', ',')}";
      }
      else if (0 == string.Compare(ComboBoxConstants.kPredefinedChecks, ClangTidyUseChecksFromConvertor.ToString(aTidyOptions.UseChecksFrom)))
      {
        foreach (PropertyInfo prop in aTidyChecks.GetType().GetProperties())
        {
          object[] propAttrs = prop.GetCustomAttributes(false);
          object clangCheckAttr = propAttrs.FirstOrDefault(attr => typeof(ClangCheckAttribute) == attr.GetType());
          object displayNameAttrObj = propAttrs.FirstOrDefault(attr => typeof(DisplayNameAttribute) == attr.GetType());

          if (null == clangCheckAttr || null == displayNameAttrObj)
            continue;

          DisplayNameAttribute displayNameAttr = (DisplayNameAttribute)displayNameAttrObj;
          var value = prop.GetValue(aTidyChecks, null);
          if (Boolean.TrueString != value.ToString())
            continue;
          parameters = $"{parameters},{displayNameAttr.DisplayName}";
        }
      }

      if (string.Empty != parameters)
      {
        parameters = string.Format("{0} ''{1}{2}''",
          (true == aTidyFixFlag ? ScriptConstants.kTidyFix : ScriptConstants.kTidy),
          (mUseTidyFile ? "" : "-*"),
          parameters);
      }

      if (null != aTidyOptions.HeaderFilter && false == string.IsNullOrWhiteSpace(aTidyOptions.HeaderFilter.HeaderFilters))
      {
        parameters = string.Format("{0} {1} ''{2}''",
          parameters, ScriptConstants.kHeaderFilter,
          true == string.IsNullOrWhiteSpace(ClangTidyHeaderFiltersConvertor.ScriptEncode(aTidyOptions.HeaderFilter.HeaderFilters)) ?
            aTidyOptions.HeaderFilter.HeaderFilters : ClangTidyHeaderFiltersConvertor.ScriptEncode(aTidyOptions.HeaderFilter.HeaderFilters));
      }

      return parameters;
    }


    #endregion

  }
}
