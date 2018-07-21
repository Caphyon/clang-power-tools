using System;
using ClangPowerTools.Error;
using ClangPowerTools.Services;
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


    #region IBuilder Implementation

    /// <summary>
    /// Build the output window model;
    /// </summary>
    public async void Build()
    {

      var outputWindowService = await mPackage.GetServiceAsync(typeof(SVsOutputWindowService)) as IVsOutputWindowService;
      mOutputWindow.VsOutputWindow = await outputWindowService.GetOutputWindowAsync(mPackage, new System.Threading.CancellationToken());

      Guid generalPaneGuid = mOutputWindow.PaneGuid;
      mOutputWindow.VsOutputWindow.GetPane(ref generalPaneGuid, out IVsOutputWindowPane pane);

      if (null == pane)
      {
        mOutputWindow.VsOutputWindow.CreatePane(ref generalPaneGuid, OutputWindowConstants.kPaneName, 0, 1);
        mOutputWindow.VsOutputWindow.GetPane(ref generalPaneGuid, out pane);
      }

      mOutputWindow.Pane = pane;
    }

    /// <summary>
    /// Get the output window model
    /// </summary>
    /// <returns></returns>
    public OutputWindowModel GetResult() => mOutputWindow;

    #endregion

  }
}
