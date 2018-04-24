using ClangPowerTools.DialogPages;
using System;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ClangPowerTools.Options.ViewModel
{
  /// <summary>
  /// Interaction logic for UserControlWPF.xaml
  /// </summary>
  public partial class ClangFormatOptionsUserControl : UserControl
  {
    public ClangFormatOptionsUserControl(ClangFormatOptionsView clangFormat)
    {
      InitializeComponent();

      wpfPropGrid.SelectedObject = clangFormat;

      Loaded += delegate
        {
          UIElementsActivator.Activate(HwndSource.FromVisual(this) as HwndSource);
        };
    }

  }
}
