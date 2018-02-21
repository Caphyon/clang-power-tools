using System;
using Microsoft.VisualStudio.Shell;
using EnvDTE80;
using EnvDTE;
using System.Collections.Generic;

namespace ClangPowerTools
{
  public abstract class BasicCommand
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
    protected Package Package { get; set; }
    protected IServiceProvider ServiceProvider => Package;
    protected DTE2 DTEObj { get; set; }

    #endregion

    #region Constructor

    protected BasicCommand(Package aPackage, Guid aGuid, int aId)
    {
      Package = aPackage ?? throw new ArgumentNullException("package");
      CommandSet = aGuid;
      Id = aId;

      DTEObj = (DTE2)ServiceProvider.GetService(typeof(DTE));
      DTEObj.Events.BuildEvents.OnBuildBegin +=
        new _dispBuildEvents_OnBuildBeginEventHandler(this.OnBuildBegin);
    }

    #endregion

    #region Private Methods

    private void OnBuildBegin(EnvDTE.vsBuildScope Scope, EnvDTE.vsBuildAction Action)
    {
      ErrorsManager errorsManager = new ErrorsManager(ServiceProvider, DTEObj);
      errorsManager.Clear();
    }

    #endregion

  }
}
