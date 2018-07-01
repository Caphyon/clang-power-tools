using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools.Services
{
  public class VsRunningDocumentTableService : SVsRunningDocumentTableService, IVsRunningDocumentTableService, IService
  {
    #region Members

    private Microsoft.VisualStudio.Shell.IAsyncServiceProvider mServiceProvider;

    #endregion


    #region Constructor 

    public VsRunningDocumentTableService(Microsoft.VisualStudio.Shell.IAsyncServiceProvider aAsyncServiceProvider)
      => mServiceProvider = aAsyncServiceProvider;

    #endregion


    #region IEnvDTEService implementation

    public async Task<IVsRunningDocumentTable> GetVsRunningDocumentTableAsync(
      Microsoft.VisualStudio.Shell.IAsyncServiceProvider provider, CancellationToken cancellationToken)
    {
      return await provider.GetServiceAsync(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
    }

    #endregion

  }
}
