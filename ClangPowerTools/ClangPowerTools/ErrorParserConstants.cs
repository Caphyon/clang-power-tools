using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class ErrorParserConstants
  {
    #region Constants

    public const string kClangTag = "Clang : ";
    public const string kEndErrorsTag = "errors generated";
    public const string kErrorTag = "Error:";
    public const string kNoteTag = "note:";
    public static readonly List<string> kExtensionsTag = new List<string>() { ".cpp", ".h" };
    
    #endregion
  }
}
