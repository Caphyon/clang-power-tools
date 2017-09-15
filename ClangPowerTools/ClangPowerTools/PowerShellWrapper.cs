using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;

namespace ClangPowerTools
{
  public class PowerShellWrapper
  {
    #region Members

    private PowerShell mPowerShell;
    private List<ScriptError> mErrors = new List<ScriptError>();
    private List<string> mOutput = new List<string>();

    #endregion

    #region Properties

    public List<ScriptError> GetErrors => mErrors;
    public List<string> GetOutput => mOutput;

    #endregion

    #region Public Methods

    public void Invoke(List<Tuple<IItem, IVsHierarchy>> aItems, ScriptBuiler aScriptBuilder, IServiceProvider aServiceProvider)
    {
      foreach (var itm in aItems)
      {
        using (mPowerShell = PowerShell.Create())
        {
          try
          {
            string script = aScriptBuilder.GetScript(itm.Item1, itm.Item1.GetName());
            mPowerShell.AddScript(script);

            Collection<PSObject> PSOutput;
            using (var guard = new SilentFileChangerGuard(aServiceProvider, itm.Item1.GetPath(), true))
            {
              PSOutput = mPowerShell.Invoke();
            }

            ErrorParser errorParser = new ErrorParser(itm.Item2);
            errorParser.Start(PSOutput);
            mErrors.AddRange(errorParser.Errors);
            mOutput.AddRange(errorParser.Output);
            mOutput.Add(String.Join("\n", mPowerShell.Streams.Error
              .Where(err => !string.IsNullOrWhiteSpace(err.ToString()))));
          }
          catch (RuntimeException exception)
          {
            VsShellUtilities.ShowMessageBox((IServiceProvider)this, exception.Message, "Error",
              OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
          }
        }
      }
    }

    #endregion
    
  }
}
