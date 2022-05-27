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
    public static string CalledExprDefaultArg
    {
      get 
      {
        return "match callExpr(callee(functionDecl(hasName(\"{0}\"))), hasArgument({1}, cxxDefaultArgExpr()))";
      }
    }

  }

}
