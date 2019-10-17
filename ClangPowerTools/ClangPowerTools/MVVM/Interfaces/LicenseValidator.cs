using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Interfaces
{
  public abstract class LicenseValidator : ILicense
  {
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

    public abstract bool Validate();

  }
}
