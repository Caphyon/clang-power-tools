using System.Windows.Controls;
using System.Windows.Interop;

namespace ClangPowerTools.Options.View
{
  /// <summary>
  /// Interaction logic for ClangTidyChecksOptionsUserControl.xaml
  /// </summary>
  public partial class ClangTidyChecksOptionsUserControl : UserControl
  {
    public ClangTidyChecksOptionsUserControl(ClangTidyPredefinedChecksOptionsView clangTidyChecksOptions)
    {
      InitializeComponent();

      wpfPropGrid.SelectedObject = clangTidyChecksOptions;

      Loaded += delegate
      {
        UIElementsActivator.Activate(HwndSource.FromVisual(this) as HwndSource);
      };

    }
  }
}
