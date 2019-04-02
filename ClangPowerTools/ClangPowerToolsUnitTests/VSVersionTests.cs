using Xunit;

namespace ClangPowerTools.Tests
{
  [VsTestSettings(UIThread = true)]
  public class VSVersionTests
  {
    [VsFact(Version = "2012")]
    public void TestMethod11()
    {
      Assert.Equal("11.0", UnitTestUtility.GetVsVersion());
    }

    [VsFact(Version = "2013")]
    public void TestMethod12()
    {
      //  Assert.Equal("12.0", DteUtilities.GetVsVersion());
    }

    [VsFact(Version = "2015")]
    public void TestMethod14()
    {
      //Assert.Equal("14.0", DteUtilities.GetVsVersion());
    }

    [VsFact(Version = "2017")]
    public void TestMethod15()
    {
      Assert.Equal("15.0", UnitTestUtility.GetVsVersion());
    }

    [VsFact(Version = "2019")]
    public void TestMethod16()
    {
      Assert.Equal("16.0", UnitTestUtility.GetVsVersion());
    }
  }
}
