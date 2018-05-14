namespace ClangPowerTools
{
  public class DefaultOptions
  {
    public const string kClangFlags = "-Werror;-Wall;-fms-compatibility-version=19.10;-Wmicrosoft;-Wno-invalid-token-paste;-Wno-unknown-pragmas;-Wno-unused-value";
    public const string kHeaderFilter = ".*";
    public const string kFileExtensions = ".c;.cpp;.cxx;.cc;.tli;.tlh;.h;.hh;.hpp;.hxx;.hh;";
    public const string kSkipFiles = "resource.h;";
  }
}
