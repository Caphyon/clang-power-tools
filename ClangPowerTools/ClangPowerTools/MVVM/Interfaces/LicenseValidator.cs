using System.IO;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Interfaces
{
  /// <summary>
  /// Contains the logic of token retrival letting license validation logic
  /// to be implemented in the extended classes.
  /// </summary>
  public abstract class LicenseValidator : ILicense
  {
    #region Members

    /// <summary>
    /// License token name
    /// </summary>
    private readonly string fileName = "ctpjwt";

    #endregion

    #region ILicense Implementation

    /// <summary>
    /// Get the license token path
    /// </summary>
    /// <returns>License token path if the file exists. Empty string otherwise.</returns>
    public string GetTokenPath()
    {
      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      string filePath = settingsPathBuilder.GetPath(fileName);

      return File.Exists(filePath) == true ? filePath : string.Empty;
    }

    /// <summary>
    /// Get the content of the license token
    /// </summary>
    /// <returns>Content of license token</returns>
    public string GetToken()
    {
      var filePath = GetTokenPath();
      if (string.IsNullOrWhiteSpace(filePath))
        return string.Empty;

      using var streamReader = new StreamReader(filePath);
      var jwt = streamReader.ReadLine();

      return jwt;
    }

    /// <summary>
    /// Check if the user license is active
    /// </summary>
    /// <returns>True if the license is active. False otherwise</returns>
    public abstract Task<bool> ValidateAsync();

    #endregion

  }
}
