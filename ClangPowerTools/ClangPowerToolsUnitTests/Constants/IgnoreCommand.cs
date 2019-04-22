using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerToolsUnitTests.Constants
{
  public class IgnoreCommand
  {
    public static readonly List<string> kSingleFileToIgnore = new List<string>()
    {
      @"DispatcherHandler.cpp",
    };

    public static readonly List<string> kMultipleFilesToIgnore = new List<string>()
    {
      @"DispatcherHandler.cpp",
      @"VsServiceProviderTests.cpp",
      @"AsyncPackageTests.cpp"
    };

    public static readonly List<string> kStartUpMultipleFilesToIgnore = new List<string>()
    {
      @"CommandController.cpp",
      @"Settings.cpp",
      @"Options.cpp"
    };
  }
}
