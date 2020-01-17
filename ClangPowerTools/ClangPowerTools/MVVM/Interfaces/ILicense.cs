using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Interfaces
{
  /// <summary>
  /// Contains license validation and token retrival main behavior
  /// </summary>
  public interface ILicense
  {
    /// <summary>
    /// Check if the user license is active
    /// </summary>
    /// <returns>True if the license is active. False otherwise</returns>
    Task<bool> ValidateAsync();

    /// <summary>
    /// Get the user license token value
    /// </summary>
    /// <returns>User license token value</returns>
    bool GetToken(out string jwt);
  }
}
