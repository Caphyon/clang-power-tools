using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClangPowerTools
{
  public class Vs15SolutionLoader
  {
    #region Members

    private IServiceProvider mServiceProvider;

    #endregion

    #region Constructor

    public Vs15SolutionLoader(IServiceProvider aServiceProvider) => mServiceProvider = aServiceProvider;

    #endregion

    #region Public Methods

    public void EnsureSolutionProjectsAreLoaded()
    {
      if (!IsSolutionLoadedDeferred())
        return;

      var projects = GetProjectsInSolution(mServiceProvider).ToList();
      foreach (var prj in projects)
        EnsureProjectIsLoaded(prj, mServiceProvider);
    }

    public IEnumerable<IVsHierarchy> GetProjectsInSolution(IServiceProvider aServiceProvider) =>
      GetProjectsInSolution(aServiceProvider, __VSENUMPROJFLAGS.EPF_ALLPROJECTS);

    #endregion

    #region Private Methods

    private IVsHierarchy EnsureProjectIsLoaded(IVsHierarchy projectToLoad, IServiceProvider aServiceProvider)
    {
      int hr = VSConstants.S_OK;
      var solution = aServiceProvider.GetService(typeof(IVsSolution)) as IVsSolution;

      hr = projectToLoad.GetGuidProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_ProjectIDGuid, out Guid projectGuid);
      hr = ErrorHandler.ThrowOnFailure(hr);
      hr = ((IVsSolution4)solution).EnsureProjectIsLoaded(projectGuid, (uint)__VSBSLFLAGS.VSBSLFLAGS_None);
      hr = ErrorHandler.ThrowOnFailure(hr);

      hr = ((IVsSolution)solution).GetProjectOfGuid(projectGuid, out IVsHierarchy loadedProject);
      hr = ErrorHandler.ThrowOnFailure(hr);

      return loadedProject;
    }

    private IEnumerable<IVsHierarchy> GetProjectsInSolution(IServiceProvider aServiceProvider, __VSENUMPROJFLAGS flags)
    {
      IVsSolution solution = aServiceProvider.GetService(typeof(IVsSolution)) as IVsSolution;

      if (null == solution)
        yield break;

      Guid guid = Guid.Empty;
      solution.GetProjectEnum((uint)flags, ref guid, out IEnumHierarchies enumHierarchies);
      if ( null == enumHierarchies )
        yield break;

      IVsHierarchy[] hierarchy = new IVsHierarchy[1];
      while ( VSConstants.S_OK == enumHierarchies.Next(1, hierarchy, out uint fetched)  && 1 == fetched )
      {
        if (hierarchy.Length > 0 && hierarchy[0] != null)
          yield return hierarchy[0];
      }
    }

    private bool IsSolutionLoadedDeferred()
    {
      IVsSolution7 vsSolution = mServiceProvider.GetService(typeof(SVsSolution)) as IVsSolution7;
      return vsSolution.IsSolutionLoadDeferred();
    }

    #endregion

  }
}
