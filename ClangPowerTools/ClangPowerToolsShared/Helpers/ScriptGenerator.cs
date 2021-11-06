using ClangPowerTools.Builder;
using ClangPowerTools.Script;
using ClangPowerToolsShared.Commands.Models;
using System.Collections.Generic;
using System.Text;

namespace ClangPowerTools.Helpers
{
  public static class ScriptGenerator
  {
    #region Methods

    public static string GetRunModeParamaters()
    {
      IBuilder<string> runModeScriptBuilder = new RunModeScriptBuilder();
      runModeScriptBuilder.Build();
      var runModeParameters = runModeScriptBuilder.GetResult();
      return runModeParameters;
    }

    public static string GetGenericParamaters(int aCommandId, string vsEdition, string vsVersion, bool jsonCompilationDbActive)
    {
      IBuilder<string> genericScriptBuilder = new GenericScriptBuilder(vsEdition, vsVersion, aCommandId, jsonCompilationDbActive);
      genericScriptBuilder.Build();
      var genericParameters = genericScriptBuilder.GetResult();
      return genericParameters;
    }

    public static string GetItemRelatedParametersCustomPaths(List<string> filePaths, CacheProjectsItemsModel cacheProjectsItemsModel)
    {
      //    var projectData = jsonCompilationDbActive ?
      //string.Empty : $"{ScriptConstants.kProject} ''{containingProject}'' ";
      string script = $"{ScriptConstants.kProject} {JoinPathsToStringScript(cacheProjectsItemsModel.ProjectsStringList)} {ScriptConstants.kFile} {JoinPathsToStringScript(filePaths)} {ScriptConstants.kActiveConfiguration} ''{cacheProjectsItemsModel.Configuration}|{cacheProjectsItemsModel.Platform}''";
      return script;
    }

    private static string JoinPathsToStringScript(List<string> paths)
    {
      StringBuilder stringBuilder = new StringBuilder("(''");
      foreach (string path in paths)
      {
        stringBuilder.Append(path).Append("'',''");
      }
      stringBuilder.Remove(stringBuilder.Length - 3, 3);
      return stringBuilder.Append(")").ToString();
    }

    public static string GetItemRelatedParameters(IItem item, bool jsonCompilationDbActive = false)
    {
      IBuilder<string> itemRelatedScriptBuilder = new ItemRelatedScriptBuilder(item, jsonCompilationDbActive);
      itemRelatedScriptBuilder.Build();
      var itemRelatedParameters = itemRelatedScriptBuilder.GetResult();
      return itemRelatedParameters;
    }

    public static string GetItemRelatedParameters(List<IItem> items, bool jsonCompilationDbActive = false)
    {
      IBuilder<string> itemRelatedScriptBuilder = new ItemRelatedScriptBuilder(items, jsonCompilationDbActive);
      itemRelatedScriptBuilder.Build();
      var itemRelatedParameters = itemRelatedScriptBuilder.GetResult();
      return itemRelatedParameters;
    }

    #endregion


  }
}
