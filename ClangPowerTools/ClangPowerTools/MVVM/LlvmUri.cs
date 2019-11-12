using ClangPowerTools.MVVM.Constants;
using System;

namespace ClangPowerTools.MVVM
{
  public class LlvmUri
  {

    public string GetDefaultUri(string version)
    {
      return string.Concat(LlvmConstants.ReleasesUri, "/", version, "/", LlvmConstants.Llvm, "-", version, GetOperatingSystemParamaters());
    }

    public string GetGitHubUri(string version)
    {
      return string.Concat(LlvmConstants.GitHubUri, "/llvmorg-", version, "/", LlvmConstants.Llvm, "-", version, GetOperatingSystemParamaters());
    }

    private string GetOperatingSystemParamaters()
    {
      return Environment.Is64BitOperatingSystem ? LlvmConstants.Os64Paramater : LlvmConstants.Os32Paramater;
    }
  }
}
