using System.ComponentModel.Design;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace ClangPowerTools.Services.OleMenuCommandCustomService
{
  class OleMenuCommandServiceImpl : SOleMenuCommandService, IOleMenuCommandService, IService
  {
    #region Members

    private IAsyncServiceProvider mServiceProvider;

    #endregion


    #region Constructor 

    public OleMenuCommandServiceImpl(IAsyncServiceProvider aAsyncServiceProvider)
      => mServiceProvider = aAsyncServiceProvider;

    #endregion


    #region IEnvDTEService implementation

    public async Task<OleMenuCommandService> GetVsFileChangeAsync(IAsyncServiceProvider provider, CancellationToken cancellationToken)
    {
      return await provider.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
    }

    #endregion

  }
}
