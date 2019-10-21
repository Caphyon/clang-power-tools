using System.IO;

namespace ClangPowerTools.MVVM.Interfaces
{
  public abstract class LicenseValidator : ILicense
  {
    #region Members

    private readonly string fileName = "ctpjwt";

    #endregion

    #region ILicense Implementation

    public string GetToken()
    {
      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      string filePath = settingsPathBuilder.GetPath(fileName);

      if (File.Exists(filePath) == false)
        return string.Empty;

      using var streamReader = new StreamReader(filePath);
      var jwt = streamReader.ReadLine();

      return jwt;
    }

    public abstract bool Validate();

    #endregion

  }
}
