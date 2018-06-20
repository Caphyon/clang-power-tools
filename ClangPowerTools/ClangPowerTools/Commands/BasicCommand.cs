using System;
using EnvDTE80;
using EnvDTE;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;

namespace ClangPowerTools
{

  /// <summary>
  /// Marks a type as requiring asynchronous initialization and provides the result of that initialization.
  /// </summary>
  public interface IAsyncInitialization
  {
    /// <summary>
    /// The result of the asynchronous initialization of this instance.
    /// </summary>
    System.Threading.Tasks.Task Initialization { get; }
  }

  public abstract class BasicCommand : IAsyncInitialization
  {
    #region Members

    private Dictionary<string, string> mVsVersions = new Dictionary<string, string>
    {
      {"11.0", "2010"},
      {"12.0", "2012"},
      {"13.0", "2013"},
      {"14.0", "2015"},
      {"15.0", "2017"}
    };

    #endregion

    #region Properties

    protected int Id { get; set; }
    protected Guid CommandSet { get; set; }
    protected AsyncPackage AsyncPackage { get; set; }
    protected Package Package => AsyncPackage;
    protected IAsyncServiceProvider ServiceProvider => AsyncPackage;
    protected DTE2 DTEObj { get; set; }

    public System.Threading.Tasks.Task Initialization { get; private set; }


    #endregion

    #region Constructor


    public async System.Threading.Tasks.Task Init(AsyncPackage aPackage, Guid aGuid, int aId)
    {
      AsyncPackage = aPackage ?? throw new ArgumentNullException("AsyncPackage");
      CommandSet = aGuid;
      Id = aId;

      DTEObj = await ServiceProvider.GetServiceAsync(typeof(DTE)) as DTE2;
      
    }

    #endregion

  }



}
