using System.Threading;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools.Services
{
  public class VsSolutionService : SVsSolutionService, IVsSolutionService
  {
    #region Members

    private Microsoft.VisualStudio.Shell.IAsyncServiceProvider mServiceProvider;

    #endregion


    #region Constructor 

    public VsSolutionService() { }


    public VsSolutionService(Microsoft.VisualStudio.Shell.IAsyncServiceProvider aAsyncServiceProvider)
      => mServiceProvider = aAsyncServiceProvider;

    #endregion


    #region IEnvDTEService implementation

    public async System.Threading.Tasks.Task<IVsSolution> GetVsSolutionAsync(Microsoft.VisualStudio.Shell.IAsyncServiceProvider provider, CancellationToken cancellationToken)
    {
      return await provider.GetServiceAsync(typeof(SVsSolution)) as IVsSolution;
    }

    #endregion

  }
}
