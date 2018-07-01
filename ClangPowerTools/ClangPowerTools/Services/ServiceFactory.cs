using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        if (typeof(SVsFileChangeService) == serviceType)
          service = new VsFileChangeService(mServiceProvider);

        if (typeof(SVsRunningDocumentTableService) == serviceType)
          service = new VsRunningDocumentTableService(mServiceProvider);

        if (typeof(SVsSolutionService) == serviceType)
          service = new VsSolutionService(mServiceProvider);

        if (typeof(SVsStatusBarService) == serviceType)
          service = new VsStatusBarService(mServiceProvider);
      });

      return service;
    }

    #endregion


  }
}
