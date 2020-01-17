namespace ClangPowerTools.Events
{
  public class LicenseEventArgs
  {
    public bool IsLicenseActive { get; set; }
    public bool TokenExists { get; set; }

    public LicenseEventArgs(bool isLicenseActive, bool tokenExists)
    {
      IsLicenseActive = isLicenseActive;
      TokenExists = tokenExists;
    }
  }
}
