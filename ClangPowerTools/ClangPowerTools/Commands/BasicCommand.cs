using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
