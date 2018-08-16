using System;
using System.Threading.Tasks;
using ClangPowerTools.Builder;
using ClangPowerTools.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ClangPowerTools.Output
{
  public class OutputWindowBuilder : IAsyncBuilder<OutputWindowModel>
  {
    #region Private Members

    /// <summary>
    /// Output window model instance
    /// </summary>
    private OutputWindowModel mOutputWindow = new OutputWindowModel();


    /// <summary>
    /// Async Package will be used to get the VS output window instance
    /// </summary>
    private AsyncPackage mPackage;


    #endregion


    #region Constructor


    /// <summary>
    /// Instance constructor
    /// </summary>
    /// <param name="aDte"></param>
    public OutputWindowBuilder(AsyncPackage aPackage) => mPackage = aPackage;


    #endregion


    #region IAsyncBuilder Implementation


    public async Task<object> AsyncBuild()
    {
      var outputWindowService = await mPackage.GetServiceAsync(typeof(SVsOutputWindowService)) as IVsOutputWindowService;

      // Get the VS Output Window 
      if (null == mOutputWindow.VsOutputWindow)
      {
        mOutputWindow.VsOutputWindow = await outputWindowService.GetOutputWindowAsync(mPackage, new System.Threading.CancellationToken());
      }

      if (null == mOutputWindow.Pane)
      {
        // Get the Pane object
        Guid generalPaneGuid = mOutputWindow.PaneGuid;
        mOutputWindow.VsOutputWindow.GetPane(ref generalPaneGuid, out IVsOutputWindowPane pane);

        // If pane does not exists, create it
        if (null == pane)
        {
          mOutputWindow.VsOutputWindow.CreatePane(ref generalPaneGuid, OutputWindowConstants.kPaneName, 0, 1);
          mOutputWindow.VsOutputWindow.GetPane(ref generalPaneGuid, out pane);
        }
        mOutputWindow.Pane = pane;
      }
      return outputWindowService;
    }

    public OutputWindowModel GetAsyncResult() => mOutputWindow;

    //public OutputWindowModel GetAsyncResult => mOutputWindow;


    #endregion

  }
}
