using System;
using EnvDTE80;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;

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

    protected AsyncPackage AsyncPackage { get; set; }

    protected Package Package => AsyncPackage;

    protected IAsyncServiceProvider AsyncServiceProvider => AsyncPackage;

    protected IServiceProvider ServiceProvider => Package;

    protected DTE2 DTEObj { get; set; }

    #endregion

    #region Constructor


    protected BasicCommand(DTE2 aDte, AsyncPackage aPackage, Guid aGuid, int aId)
    {
      AsyncPackage = aPackage ?? throw new ArgumentNullException("AsyncPackage");
      CommandSet = aGuid;
      Id = aId;
      DTEObj = aDte;
    }

    #endregion

  }



}
