using ClangPowerTools.Builder;
using System;

namespace ClangPowerTools.Script
{
  public class VerbosityScriptBuilder : IBuilder<string>
  {
    string mVerbosity = string.Empty;
    string mResultScript = string.Empty;

    public VerbosityScriptBuilder(string aVerbosity) => mVerbosity = 
      Convert.ToString(int.Parse(aVerbosity) + 1);

    public void Build()
    {
      mResultScript = "";
      bool finishBuild = false;

      if (mVerbosity != "1" && !finishBuild)
      {
        mResultScript += $" {ScriptConstants.kVerboseMode}";
      }
      else if (mVerbosity == "1")
      {
        finishBuild = true;
      }

      if (mVerbosity != "2" && !finishBuild)
      {
        mResultScript += $" {ScriptConstants.kInformationMode} {ScriptConstants.kStringContinue} " +
          $"{ScriptConstants.kDebugMode} {ScriptConstants.kWaringMode} {ScriptConstants.kStringContinue}";
      }
    }

    public string GetResult()
    {
      return mResultScript;
    }
  }
}
