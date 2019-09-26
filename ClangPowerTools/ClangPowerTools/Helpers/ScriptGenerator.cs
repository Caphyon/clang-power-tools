using ClangPowerTools.Builder;
using ClangPowerTools.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public static string GetGenericParamaters(int aCommandId, string vsEdition, string vsVersion)
    {
      IBuilder<string> genericScriptBuilder = new GenericScriptBuilder(vsEdition, vsVersion, aCommandId);
      genericScriptBuilder.Build();
      var genericParameters = genericScriptBuilder.GetResult();
      return genericParameters;
    }

    public static string GetItemRelatedParameters(IItem item)
    {
      IBuilder<string> itemRelatedScriptBuilder = new ItemRelatedScriptBuilder(item);
      itemRelatedScriptBuilder.Build();
      var itemRelatedParameters = itemRelatedScriptBuilder.GetResult();
      return itemRelatedParameters;
    }

    #endregion


  }
}
