using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Text;

namespace ClangPowerTools
{
  public class OutputWindow : TextWriter
  {
    #region Members

    private static readonly Guid mPaneGuid = new Guid("AB9F45E4-2001-4197-BAF5-4B165222AF29");
    private static IVsOutputWindow mOutputWindow = null;
    private static IVsOutputWindowPane mOutputPane = null;
    
    #endregion

    #region Ctor

    public OutputWindow(DTE2 aDte)
    {
      if( null == mOutputWindow )
      {
        IServiceProvider serviceProvider = 
          new ServiceProvider(aDte as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
        mOutputWindow = serviceProvider.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
      }

      if (null == mOutputPane)
      {
        Guid generalPaneGuid = mPaneGuid;
        mOutputWindow.GetPane(ref generalPaneGuid, out IVsOutputWindowPane pane);
        
        if ( null == pane)
        {
          mOutputWindow.CreatePane(ref generalPaneGuid, OutputWindowConstants.kPaneName, 3, 1);
          mOutputWindow.GetPane(ref generalPaneGuid, out pane);
        }
        mOutputPane = pane;
      }
    }

    #endregion

    #region Properties

    public override Encoding Encoding => System.Text.Encoding.Default;

    #endregion

    #region Public Methods

    public override void Write(string aMessage) => mOutputPane.OutputString($"{aMessage}\n");

    public override void Write(char aCharacter) => mOutputPane.OutputString(aCharacter.ToString());

    public void Show(DTE2 aDte)
    {
      mOutputPane.Activate();
      aDte.ExecuteCommand("View.Output", string.Empty);
    }

    public void Clear() => mOutputPane.Clear();

    #endregion
  }
}
