using System;
using System.Collections.Generic;

namespace ClangPowerTools.Services
{
  /// <summary>
  /// Contains all the logic of store and get VS services
  /// </summary>
  public static class VsServiceProvider
  {
    #region Members

    /// <summary>
    /// VS Services collection
    /// </summary>
    private static Dictionary<Type, object> mServices = new Dictionary<Type, object>();

    #endregion


    #region Public Methods

    /// <summary>
    /// Store the service
    /// </summary>
    /// <param name="aType">The type of the service</param>
    /// <param name="aService">The service instance</param>
    public static void Register(Type aType, object aService) => mServices.Add(aType, aService);


    /// <summary>
    /// Get the VS service. Throws an exception if the required service was not registered before.
    /// </summary>
    /// <param name="aType">The type of the service</param>
    /// <returns>The VS service object</returns>
    public static object GetService(Type aType) => mServices[aType];


    /// <summary>
    /// Get the VS service. If the wanted service was not registered before the out parameter will be null.
    /// </summary>
    /// <param name="aType">The type of the service</param>
    /// <param name="aService">The wanted VS service object</param>
    /// <returns>True if the service was registered before. False otherwise</returns>
    public static bool TryGetService(Type aType, out object aService)
    {
      return mServices.TryGetValue(aType, out aService);
    }


    #endregion

  }
}
