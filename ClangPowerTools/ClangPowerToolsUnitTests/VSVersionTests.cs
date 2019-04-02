using Xunit;

namespace ClangPowerTools.Tests
{
  [VsTestSettings(UIThread = true)]
  public class VSVersionTests
  {
    [VsFact(Version = "2015")]
    public void RunVisualStudio2015_Test()
    {
      Assert.Equal("14.0", UnitTestUtility.GetVsVersion());
    }

    [VsFact(Version = "2017")]
    public void RunVisualStudio2017_Test()
    {
      Assert.Equal("15.0", UnitTestUtility.GetVsVersion());
    }

    [VsFact(Version = "2019")]
    public void RunVisualStudio2019_Test()
    {
      Assert.Equal("16.0", UnitTestUtility.GetVsVersion());
    }
  }
}
