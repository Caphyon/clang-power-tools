namespace ClangPowerTools.Events
{
  public class LicenseEventArgs
  {
    public bool IsLicenseActive { get; set; }

    public LicenseEventArgs(bool isLicenseActive)
    {
      IsLicenseActive = isLicenseActive;
    }
  }
}
