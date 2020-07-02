using ClangPowerTools.MVVM.Constants;
using ClangPowerTools.MVVM.Models;
using System;

namespace ClangPowerTools.MVVM.LicenseValidation
{
  public class LicenseInfo
  {
    #region Properties

    /// <summary>
    /// License model created after the web api request
    /// </summary>
    public LicenseModel LicenseModel { get; private set; }

    /// <summary>
    /// License expiration status
    /// </summary>
    public LicenseState State { get; private set; }

    /// <summary>
    /// Active license type: Commercial, Personal, Trial
    /// </summary>
    public Constants.LicenseType Type { get; private set; }

    #endregion

    #region Constructor

    public LicenseInfo(LicenseModel licenseModel)
    {
      LicenseModel = licenseModel;
      SetLicenseState();
      SetLicenseType();
    }


    #endregion

    #region Private Methods

    private void SetLicenseState()
    {
      DateTime expirationDate = DateTime.Parse(LicenseModel.expires);
      State = DateTime.Now.Subtract(expirationDate).Days >= 1 ?
        LicenseState.Expired : LicenseState.Active;
    }

    private void SetLicenseType()
    {
      // Trial version is ignored at this moment

      Type = string.IsNullOrWhiteSpace(LicenseModel.expires) ?
        Constants.LicenseType.Personal : Constants.LicenseType.Commercial;
    }

    #endregion


  }
}
