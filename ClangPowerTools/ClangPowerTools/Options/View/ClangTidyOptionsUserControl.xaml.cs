using System.Windows.Controls;
using System.Windows.Interop;

namespace ClangPowerTools.Options.View
{
  /// <summary>
  /// Interaction logic for ClangTidyOptionsUserControl.xaml
  /// </summary>
  public partial class ClangTidyOptionsUserControl : UserControl
  {
    public ClangTidyOptionsUserControl(ClangTidyOptionsView clangTidyOptions)
    {
      InitializeComponent();

      wpfPropGrid.SelectedObject = clangTidyOptions;

      Loaded += delegate
      {
        UIElementsActivator.Activate(HwndSource.FromVisual(this) as HwndSource);
      };
    }
  }
}
