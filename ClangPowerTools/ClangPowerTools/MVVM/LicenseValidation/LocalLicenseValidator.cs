using ClangPowerTools.MVVM.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.LicenseValidation
{
  public class LocalLicenseValidator : LicenseValidator
  {
    #region Members

    private readonly string fileName = "ctpjwt";

    #endregion

    #region ILicense Implementaion

    public string GetToken()
    {
      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      string filePath = settingsPathBuilder.GetPath("ctpjwt");

      if (File.Exists(filePath) == false)
        return string.Empty;

      using var streamReader = new StreamReader(filePath);
      var jwt = streamReader.ReadLine();

      return jwt;
    }

    public override bool Validate()
    {
      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      string filePath = settingsPathBuilder.GetPath(fileName);

      if (File.Exists(filePath) == false)
        return false;

      using StreamReader reader = new StreamReader(filePath);
      var jwt = reader.ReadToEnd();

      return string.IsNullOrWhiteSpace(jwt) == false;
    }

    #endregion

  }
}
