using ClangPowerTools.MVVM.Interfaces;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.LicenseValidation
{
  public class LocalLicenseValidator : ILicense
  {
    #region ILicense Implementation

    /// <summary>
    /// Check the if the user license from the disk is active.
    /// </summary>
    /// <returns>True if the license is active. False otherwise</returns>
    public Task<bool> ValidateAsync()
    {
      var token = new Token();
      return Task.FromResult(token.GetToken(out _));
    }

    #endregion

  }
}
