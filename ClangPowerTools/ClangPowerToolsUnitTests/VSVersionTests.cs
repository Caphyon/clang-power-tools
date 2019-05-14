using Xunit;

namespace ClangPowerTools.Tests
{
  [VsTestSettings(UIThread = true)]
  public class VsVersionTests
  {
    #region Test Methods

    //[VsFact(Version = "2015")]
    //public void RunVisualStudio2015_Test()
    //{
    //  Assert.Equal("14.0", UnitTestUtility.GetVsVersion());
    //}

    [VsFact(Version = "2017")]
    public void RunVisualStudio2017()
    {
      Assert.Equal("15.0", UnitTestUtility.GetVsVersion());
    }

    [VsFact(Version = "2019")]
    public void RunVisualStudio2019()
    {
      Assert.Equal("16.0", UnitTestUtility.GetVsVersion());
    }

    #endregion

  }
}
