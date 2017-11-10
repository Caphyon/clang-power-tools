using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{
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

    {
      Package = aPackage ?? throw new ArgumentNullException("package");
      CommandSet = aGuid;
      Id = aId;
    }
    
    #endregion

  }
}
