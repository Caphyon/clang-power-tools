using System;
using ClangPowerTools.Error;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools.Output
{
  public class OutputWindowBuilder : IBuilder<OutputWindowModel>
  {
    #region Private Members

    /// <summary>
    /// Output window model instance
    /// </summary>
    private OutputWindowModel mOutputWindow;

    /// <summary>
    /// DTE2 instance to get the VS output window elements 
    /// </summary>
    private DTE2 mDte;

    #endregion


    #region Constructor

    /// <summary>
    /// Instance constructor
    /// </summary>
    /// <param name="aDte"></param>
    public OutputWindowBuilder(DTE2 aDte) => mDte = aDte;

    #endregion


    #region IBuilder Implementation

    /// <summary>
    /// Build the output window model;
    /// </summary>
    public void Build()
    {
      if (null == mOutputWindow)
      {
        IServiceProvider serviceProvider =
          new ServiceProvider(mDte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);

        mOutputWindow.VsOutputWindow = serviceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
      }

      if (null == mOutputWindow.Pane)
      {
        Guid generalPaneGuid = mOutputWindow.PaneGuid;
        mOutputWindow.VsOutputWindow.GetPane(ref generalPaneGuid, out IVsOutputWindowPane pane);

        if (null == pane)
        {
          mOutputWindow.VsOutputWindow.CreatePane(ref generalPaneGuid, OutputWindowConstants.kPaneName, 0, 1);
          mOutputWindow.VsOutputWindow.GetPane(ref generalPaneGuid, out pane);
        }

        mOutputWindow.Pane = pane;
      }
    }

    /// <summary>
    /// return the output window model
    /// </summary>
    /// <returns></returns>
    public OutputWindowModel GetResult() => mOutputWindow;

    #endregion

  }
}
