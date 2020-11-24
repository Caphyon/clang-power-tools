using ClangPowerTools.Helpers;
using System;

namespace ClangPowerTools.MVVM.Controllers
{
  public class FreeTrialController
  {
    #region Members

    private readonly RegistryUtility registryUtility;

    private readonly string registryName = @"Software\Caphyon\Clang Power Tools";
    private readonly string keyName = "DateTimeT";
    private readonly int trialDays = 14;
    private readonly string expiredDate = "9/12/2002 7:52:51 PM";

    #endregion

    #region Constructor

    public FreeTrialController() => registryUtility = new RegistryUtility(registryName);

    #endregion

    #region Public Methods

    public bool Start() => registryUtility.WriteCurrentUserKey(keyName, DateTime.Now.ToString());

    public bool MarkAsExpired() => registryUtility.WriteCurrentUserKey(keyName, expiredDate);

    public string GetStartTime()
    {
      if (WasEverInTrial() == false)
        return string.Empty;

      return registryUtility.ReadCurrentUserKey(keyName);
    }

    public string GetExpirationDateAsString()
    {
      var freeTrialStartTimeAsString = GetStartTime();
      if (string.IsNullOrWhiteSpace(freeTrialStartTimeAsString))
        return string.Empty;

      if (WasEverInTrial() == false)
        return string.Empty;

      var freeTrialStartTime = DateTime.Parse(freeTrialStartTimeAsString);
      var expirationDate = freeTrialStartTime.AddDays(trialDays);

      return expirationDate.ToString();
    }

    public bool IsActive()
    {
      var freeTrialStartTimeAsString = GetStartTime();

      if (string.IsNullOrWhiteSpace(freeTrialStartTimeAsString))
        return false;

      var freeTrialStartTime = DateTime.Parse(freeTrialStartTimeAsString);

      return DateTime.Now.Subtract(freeTrialStartTime).Days <= trialDays;
    }

    public bool WasEverInTrial() => registryUtility.Exists() &&
                                    !string.IsNullOrWhiteSpace(registryUtility.ReadCurrentUserKey(keyName));

    #endregion

  }
}
