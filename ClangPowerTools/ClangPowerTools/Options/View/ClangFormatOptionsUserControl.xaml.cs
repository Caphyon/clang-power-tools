using ClangPowerTools.DialogPages;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ClangPowerTools.Options.ViewModel
{
  /// <summary>
  /// Interaction logic for UserControlWPF.xaml
  /// </summary>
  public partial class ClangFormatOptionsUserControl : UserControl
  {
    public ClangFormatOptionsUserControl(ClangFormatOptionsView clangFormatOptions)
    {
      InitializeComponent();

      wpfPropGrid.SelectedObject = clangFormatOptions;

      Loaded += delegate
        {
          UIElementsActivator.Activate(HwndSource.FromVisual(this) as HwndSource);
        };
    }

  }
}
