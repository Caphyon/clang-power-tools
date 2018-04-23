using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ClangPowerTools.Options.View.WpfPropertyGrid
{
  /// <summary>
  /// Interaction logic for MultilineTextBox.xaml.
  /// </summary>
  [ProvideToolboxControl("ClangPowerTools.Options.View.WpfPropertyGrid.MultilineTextBox", true)]
  public partial class StringCollectionEditor : Window
  {

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private const int GWL_STYLE = -16;
    private const int WS_MAXIMIZEBOX = 0x10000; //maximize button
    private const int WS_MINIMIZEBOX = 0x20000; //minimize button

    private IntPtr _windowHandle;

    public StringCollectionEditor()
    {
      InitializeComponent();

      this.SourceInitialized += MainWindow_SourceInitialized;
    }

    private void MainWindow_SourceInitialized(object sender, EventArgs e)
    {
      _windowHandle = new WindowInteropHelper(this).Handle;

      // hide the buttons
      HideMinimizeAndMaximizeButtons();
    }

    protected void HideMinimizeAndMaximizeButtons()
    {
      if (_windowHandle == null)
        throw new InvalidOperationException("The window has not yet been completely initialized");

      SetWindowLong(_windowHandle, GWL_STYLE, GetWindowLong(_windowHandle, GWL_STYLE) & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
    }

    private void Button1_Click(object sender, RoutedEventArgs e)
    {
      MessageBox.Show(string.Format(CultureInfo.CurrentUICulture, "We are inside {0}.Button1_Click()", this.ToString()));
    }
  
  }
}
