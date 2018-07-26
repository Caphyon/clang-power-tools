using System.Threading;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools.Services
{
  public class VsFileChangeService : SVsFileChangeService, IVsFileChangeService, IService
  {
    #region Members

    private Microsoft.VisualStudio.Shell.IAsyncServiceProvider mServiceProvider;

    #endregion


    #region Constructor 

    public VsFileChangeService(Microsoft.VisualStudio.Shell.IAsyncServiceProvider aAsyncServiceProvider)
      => mServiceProvider = aAsyncServiceProvider;

    #endregion


    #region IEnvDTEService implementation

    public async System.Threading.Tasks.Task<IVsFileChangeEx> GetVsFileChangeAsync(
      Microsoft.VisualStudio.Shell.IAsyncServiceProvider provider, CancellationToken cancellationToken)
    {
      return await provider.GetServiceAsync(typeof(SVsFileChangeEx)) as IVsFileChangeEx;
    }

    #endregion

  }
}
