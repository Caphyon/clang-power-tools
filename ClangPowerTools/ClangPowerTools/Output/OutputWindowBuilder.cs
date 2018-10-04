using ClangPowerTools.Builder;
using ClangPowerTools.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace ClangPowerTools.Output
{
  public class OutputWindowBuilder : IBuilder<OutputWindowModel>
  {
    #region Private Members


    /// <summary>
    /// Output window model instance
    /// </summary>
    private OutputWindowModel mOutputWindowModel = new OutputWindowModel();


    /// <summary>
    /// Output window model instance
    /// </summary>
    private AsyncPackage mAsyncPackage;


    #endregion


    #region Constructor


    /// <summary>
    /// Instance constructor
    /// </summary>
    /// <param name="aDte"></param>
    public OutputWindowBuilder(AsyncPackage aPackage, IVsOutputWindow aVsOutputWindow)
    {
      mOutputWindowModel.VsOutputWindow = aVsOutputWindow;
      mAsyncPackage = aPackage;
    }


    #endregion


    #region IAsyncBuilder Implementation


    public void Build()
    {
      // Get the VS Output Window 
      if (null == mOutputWindowModel.VsOutputWindow)
      {
        if (VsServiceProvider.TryGetService(typeof(SVsOutputWindow), out object vsOutputWindow))
          mOutputWindowModel.VsOutputWindow = vsOutputWindow as IVsOutputWindow;
      }

      if (null == mOutputWindowModel.Pane)
      {
        // Get the Pane object
        Guid generalPaneGuid = mOutputWindowModel.PaneGuid;
        mOutputWindowModel.VsOutputWindow.GetPane(ref generalPaneGuid, out IVsOutputWindowPane pane);

        // If pane does not exists, create it
        if (null == pane)
        {
          mOutputWindowModel.VsOutputWindow.CreatePane(ref generalPaneGuid, OutputWindowConstants.kPaneName, 0, 1);
          mOutputWindowModel.VsOutputWindow.GetPane(ref generalPaneGuid, out pane);
        }
        mOutputWindowModel.Pane = pane;
      }
    }

    public OutputWindowModel GetResult() => mOutputWindowModel;

    #endregion

  }
}
