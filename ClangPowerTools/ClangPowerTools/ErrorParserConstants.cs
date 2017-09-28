using System.Collections.Generic;

namespace ClangPowerTools
{
  public class ErrorParserConstants
  {
    #region Constants

    public const string kClangTag                     = "Clang : ";
    public const string kEndErrorsTag                 = "errors generated";
    public const string kErrorTag                     = "Error:";
    public const string kNoteTag                      = "note:";
    public const string kCompileClangMissingFromPath  = "error: The system cannot find the file specified.";
    public const string kTidyClangMissingFromPath     = "The term 'clang-tidy' is not recognized";
    public const string kClangPathVariablesMessage    = "\n\nDid you forget to set-up LLVM?\n\nPlease follow these steps:\n- Go to http://releases.llvm.org/download.html. \n- Download the latest LLVM pre-build binaries for Windows. \n- Run installer. \n- During install, tick checkbox 'Add LLVM to the system PATH for all users/current user'. \n- Restart Visual Studio.";

    #endregion
  }
}
