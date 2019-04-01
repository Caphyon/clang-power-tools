using Xunit;

namespace ClangPowerTools.Tests
{
  [VsTestSettings(UIThread = true)]
  public class VSVersionTests
  {
    [VsFact(Version = "2012")]
    void TestMethod11()
    {
      Assert.Equal("11.0", UnitTestUtility.GetVsVersion());
    }

    [VsFact(Version = "2013")]
    void TestMethod12()
    {
      //  Assert.Equal("12.0", DteUtilities.GetVsVersion());
    }

    [VsFact(Version = "2015")]
    void TestMethod14()
    {
      //Assert.Equal("14.0", DteUtilities.GetVsVersion());
    }

    [VsFact(Version = "2017")]
    void TestMethod15()
    {
      Assert.Equal("15.0", UnitTestUtility.GetVsVersion());
    }
  }
}
