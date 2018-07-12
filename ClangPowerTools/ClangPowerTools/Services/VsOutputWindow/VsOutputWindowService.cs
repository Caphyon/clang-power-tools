using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools.Services
{
  public class VsOutputWindowService : SVsOutputWindowService, IVsOutputWindowService, IService
  {
    #region Members

    private Microsoft.VisualStudio.Shell.IAsyncServiceProvider mServiceProvider;

    #endregion


    #region Constructor 

    public VsOutputWindowService(Microsoft.VisualStudio.Shell.IAsyncServiceProvider aAsyncServiceProvider)
      => mServiceProvider = aAsyncServiceProvider;

    #endregion


    #region IEnvDTEService implementation

    public async Task<IVsOutputWindow> GetOutputWindowAsync(
      Microsoft.VisualStudio.Shell.IAsyncServiceProvider provider, CancellationToken cancellationToken)
    {
      return await provider.GetServiceAsync(typeof(SVsOutputWindow)) as IVsOutputWindow;
    }

    #endregion

  }
}
