using ClangPowerTools.Services;
using EnvDTE;
using EnvDTE80;
using Xunit;
using ClangPowerTools;
using Microsoft.VisualStudio.Shell;

namespace ClangPowerTools.Tests
{
  [VsTestSettings(UIThread = true)]
  public class VsServiceProviderTests
  {
    [VsFact()]
    public void DteServiceWasRegistered_Test()
    {

      DTE dte = VsServiceProvider.GetService(typeof(DTE)) as DTE;
      Assert.NotNull(dte);
    }


    //[VsFact()]
    //public void DTEServiceIsConvertibleToDTE2_Test()
    //{
    //  DTE2 dte2 = null;
    //  if (VsServiceProvider.TryGetService(typeof(DTE), out object dteObj))
    //  {
    //    dte2 = dteObj as DTE2;
    //  }
    //  Assert.NotNull(dte2);
    //}
  }
}
