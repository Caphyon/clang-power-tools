using ClangPowerTools.Helpers;
using System;

namespace ClangPowerTools.MVVM.Controllers
{
  public class FreeTrialController
  {
    #region Members

    private readonly RegistryUtility registryUtility;

    private readonly string registryName = @"Software\Caphyon\Cpt";
    private readonly string keyName = "Trial";
    private readonly int trialDays = 7;
    private readonly string expiredDate = "9/12/2018 7:52:51 PM";

    #endregion

    #region Constructor

    public FreeTrialController() => registryUtility = new RegistryUtility(registryName);

    #endregion

    #region Public Methods

    public bool Start() => registryUtility.WriteKey(keyName, DateTime.Now.ToString());

    public bool MarkAsExpired() => registryUtility.WriteKey(keyName, expiredDate);

    public bool IsActive()
    {
      if(WasEverInTrial() == false)
      {
        return false;
      }

      var freeTrialStartTimeAsString = registryUtility.ReadKey(keyName);
      var freeTrialStartTime = DateTime.Parse(freeTrialStartTimeAsString);

      return DateTime.Now.Subtract(freeTrialStartTime).Days <= trialDays;
    }

    public bool WasEverInTrial() => registryUtility.Exists(registryName);

    #endregion

  }
}
