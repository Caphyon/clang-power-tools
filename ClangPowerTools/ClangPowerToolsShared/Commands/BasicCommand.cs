using System;
using Microsoft.VisualStudio.Shell;

namespace ClangPowerTools
{
  public abstract class BasicCommand
  {
    #region Properties

    protected int Id { get; set; }

    protected Guid CommandSet { get; set; }

    protected AsyncPackage AsyncPackage { get; set; }

    protected Package Package => AsyncPackage;

    protected IAsyncServiceProvider AsyncServiceProvider => AsyncPackage;

    protected IServiceProvider ServiceProvider => Package;

    #endregion

    #region Constructor

    protected BasicCommand(AsyncPackage aPackage, Guid aGuid, int aId)
    {
      AsyncPackage = aPackage ?? throw new ArgumentNullException("AsyncPackage");
      CommandSet = aGuid;
      Id = aId;
    }

    #endregion

  }

}
