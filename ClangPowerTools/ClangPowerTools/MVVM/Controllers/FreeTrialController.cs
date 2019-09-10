using ClangPowerTools.Helpers;
using System;

namespace ClangPowerTools.MVVM.Controllers
{
  public class FreeTrialController
  {
    #region Members

    private readonly RegistryUtility registryUtility;

    private readonly string registryName = @"Software\Caphyon\cpt";
    private readonly string keyName = "trial";
    private readonly int days = -1;

    #endregion

    #region Constructor

    public FreeTrialController() => registryUtility = new RegistryUtility(registryName);

    #endregion

    #region Public Methods

    public bool Start() => registryUtility.WriteKey(keyName, DateTime.Now.ToString());

    public bool IsActive()
    {
      var freeTrialStartTimeAsString = registryUtility.ReadKey(keyName);
      var freeTrialStartTime = DateTime.Parse(freeTrialStartTimeAsString);

      return DateTime.Now.Subtract(freeTrialStartTime).Days <= days;
    }

    public bool WasEverInTrial() => registryUtility.Exists(registryName);

    #endregion

  }
}
