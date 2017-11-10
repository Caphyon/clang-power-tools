using System;
using Microsoft.VisualStudio.Shell;

namespace ClangPowerTools
{
  public abstract class BasicCommand
  {
    #region Members

    #endregion

    #region Properties

    protected int Id { get; set; }
    protected Guid CommandSet { get; set; }
    protected Package Package { get; set; }
    protected IServiceProvider ServiceProvider => Package;

    #endregion

    #region Constructor

    protected BasicCommand(Package aPackage, Guid aGuid, int aId)
    {
      Package = aPackage ?? throw new ArgumentNullException("package");
      CommandSet = aGuid;
      Id = aId;
    }
    
    #endregion

  }
}
