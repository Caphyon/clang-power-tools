using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.LicenseValidation
{
  public class LocalLicenseValidator : LicenseValidator
  {
    #region ILicense Implementation

    /// <summary>
    /// Check the if the user license from the disk is active.
    /// </summary>
    /// <returns>True if the license is active. False otherwise</returns>
    public override Task<bool> ValidateAsync()
    {
      return Task.FromResult(GetToken(out _));
    }

    #endregion

  }
}
