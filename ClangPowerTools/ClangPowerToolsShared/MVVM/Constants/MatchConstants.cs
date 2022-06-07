using ClangPowerTools.Helpers;
using System.Collections.Generic;

namespace ClangPowerToolsShared.MVVM.Constants
{
  /// <summary>
  /// AST Matchers
  /// </summary>
  public static class MatchConstants
  {
    /// <summary>
    /// Match a called expersion with a default argument on a position.
    /// Replace {0} with expersion name.
    /// Replace {1} with position of default arg
    /// </summary>
    public const string CalledExprDefaultArg = "match callExpr(callee(functionDecl(hasName(\"{0}\"))), hasArgument({1}, cxxDefaultArgExpr()))";

    /// <summary>
    /// Diagnostic mode can be re-entered with set output diag for source code exploration
    /// </summary>
    public const string SetOutpuDump = "set output dump";

  }

}
