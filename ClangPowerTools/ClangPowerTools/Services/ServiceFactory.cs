using ClangPowerTools.Services.OleMenuCommandCustomService;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClangPowerTools.Services
{
  /// <summary>
  /// Provides the custom service create operation and logic
  /// </summary>
  public class ServiceFactory
  {
    #region Members

    /// <summary>
    /// The service provider 
    /// </summary>
    IAsyncServiceProvider mServiceProvider;

    #endregion


    #region Constructor

    /// <summary>
    /// Instance constructor
    /// </summary>
    /// <param name="aServiceProvider">The service provider</param>
    public ServiceFactory(IAsyncServiceProvider aServiceProvider)
      => mServiceProvider = aServiceProvider;

    #endregion

    #region Public Methods

    /// <summary>
    /// Create the wanted service
    /// </summary>
    /// <param name="container">Provides methods to add and remove services</param>
    /// <param name="cancellationToken">Propagate notification that operation should be canceled</param>
    /// <param name="serviceType">The current service type</param>
    /// <returns>The service of type serviceType</returns>
    public async Task<object> CreateService(IAsyncServiceContainer container, CancellationToken cancellationToken, Type serviceType)
    {
      IService service = null;

      await System.Threading.Tasks.Task.Run(() =>
      {
        if (typeof(SEnvDTEService) == serviceType)
          service = new EnvDTEService(mServiceProvider);

        else if (typeof(SVsFileChangeService) == serviceType)
          service = new VsFileChangeService(mServiceProvider);

        else if (typeof(SVsRunningDocumentTableService) == serviceType)
          service = new VsRunningDocumentTableService(mServiceProvider);

        else if (typeof(SVsSolutionService) == serviceType)
          service = new VsSolutionService(mServiceProvider);

        else if (typeof(SVsStatusBarService) == serviceType)
          service = new VsStatusBarService(mServiceProvider);

        else if (typeof(SVsOutputWindowService) == serviceType)
          service = new VsOutputWindowService(mServiceProvider);

        else if (typeof(SOleMenuCommandService) == serviceType)
          service = new OleMenuCommandServiceImpl(mServiceProvider);

      });

      return service;
    }

    #endregion


  }
}
