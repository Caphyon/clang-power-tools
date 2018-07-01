using System.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace ClangPowerTools.Services
{
  public class EnvDTEService : SEnvDTEService, IEnvDTEService, IService
  {
    #region Members

    private IAsyncServiceProvider mServiceProvider;

    #endregion


    #region Constructor 

    public EnvDTEService(IAsyncServiceProvider aAsyncServiceProvider)
      => mServiceProvider = aAsyncServiceProvider;

    #endregion


    #region IEnvDTEService implementation

    public async System.Threading.Tasks.Task<DTE2> GetDTE2Async(IAsyncServiceProvider provider, CancellationToken cancellationToken)
    {
      return await provider.GetServiceAsync(typeof(DTE)) as DTE2;
    }

    #endregion

  }
}
