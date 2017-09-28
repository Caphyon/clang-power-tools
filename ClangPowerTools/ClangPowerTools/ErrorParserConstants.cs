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
    public const string kClangPathVariablesMessage    = "\n\nDid you forget to install LLVM and set the bin directory to the system path variables?\n\nYou can do this by following the next steps:\n\n1. Please download LLVM from here : http://releases.llvm.org/download.html \n2. Please run the executable file of LLVM.\n3. After the install window appears please press: Next -> I Agree -> Check: Add LLVM to the system PATH for all users/current user -> Next -> Next -> Install -> Finish -> Restart Visual Studio.";
    
    #endregion
  }
}
