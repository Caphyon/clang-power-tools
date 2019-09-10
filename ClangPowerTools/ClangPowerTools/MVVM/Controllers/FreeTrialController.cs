using ClangPowerTools.Helpers;
using System;

namespace ClangPowerTools.MVVM.Controllers
{
  public class FreeTrialController
  {
    #region Members

    private readonly RegistryUtility registryUtility;

    private readonly string registerName = @"Software\Caphyon\cpt";
    private readonly string keyName = "trial";
    private readonly int days = -1;

    #endregion

    #region Constructor

    public FreeTrialController() => registryUtility = new RegistryUtility(registerName);

    #endregion

    #region Public Methods

    public bool Start() => registryUtility.WriteRegistryKey(keyName, DateTime.Now.ToString());

    public bool IsActive()
    {
      var freeTrialStartTimeAsString = registryUtility.ReadRegistryKey(keyName);
      var freeTrialStartTime = DateTime.Parse(freeTrialStartTimeAsString);

      return DateTime.Now.Subtract(freeTrialStartTime).Days <= days;
    }

    #endregion

  }
}
