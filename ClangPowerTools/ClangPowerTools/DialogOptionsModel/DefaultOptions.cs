namespace ClangPowerTools
{
  public class DefaultOptions
  {
    public static readonly string[] kClangFlags = new string[]
    {
      "-Wall",
      "-fms-compatibility-version=19.10",
      "-fms-compatibility",
      "-Wmicrosoft",
      "-Wno-invalid-token-paste",
      "-Wno-unknown-pragmas",
      "-Wno-unused-variable",
      "-Wno-unused-value"
    };

    public const string kHeaderFilter = ".*";

    public const string kFileExtensions = ".c;.cpp;.cxx;.cc;.tli;.tlh;.h;.hh;.hpp;.hxx;.hh;";
    public const string kSkipFiles = "resource.h;";
  }
}
