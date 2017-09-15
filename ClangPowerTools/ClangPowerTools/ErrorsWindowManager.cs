using System;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public class ErrorsWindowManager
  {
    #region Members

    private ErrorWindow mErrorWindow = new ErrorWindow();

    #endregion

    #region Ctor

    public ErrorsWindowManager(IServiceProvider aServiceProvider) => 
      mErrorWindow.Initialize(aServiceProvider);

    #endregion

    #region Public Methods

    public void AddError(ScriptError aError)
    {
      if (!String.IsNullOrWhiteSpace(aError.ErrorMessage))
        mErrorWindow.AddError(aError);
    }

    public void AddErrors(IEnumerable<ScriptError> aErrors)
    {
      mErrorWindow.Clear();
      foreach (ScriptError error in aErrors)
        mErrorWindow.AddError(error);
    }

    #endregion
  }
}
