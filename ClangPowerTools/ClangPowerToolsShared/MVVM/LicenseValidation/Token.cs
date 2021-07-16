using System.IO;

namespace ClangPowerTools.MVVM.LicenseValidation
{
  public class Token
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
    private string GetTokenPath()
    {
      var settingsPathBuilder = new SettingsPathBuilder();
      string filePath = settingsPathBuilder.GetPath(fileName);

      return File.Exists(filePath) == true ? filePath : string.Empty;
    }

    /// <summary>
    /// Get the content of the license token
    /// </summary>
    /// <param name="jwt">Content of license token</param>
    /// <returns>True if the content of license token was succesfully extracted. False otherwise</returns>
    public bool GetToken(out string jwt)
    {
      jwt = null;
      var filePath = GetTokenPath();

      if (string.IsNullOrWhiteSpace(filePath))
        return false;

      using var streamReader = new StreamReader(filePath);
      jwt = streamReader.ReadLine();

      return string.IsNullOrWhiteSpace(jwt) == false;
    }

    #endregion

  }
}
