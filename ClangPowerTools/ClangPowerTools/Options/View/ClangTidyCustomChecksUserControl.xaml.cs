using ClangPowerTools.DialogPages;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ClangPowerTools.Options.View
{
  /// <summary>
  /// Interaction logic for ClangTidyCustomChecksUserControl.xaml
  /// </summary>
  public partial class ClangTidyCustomChecksUserControl : UserControl
  {
    public ClangTidyCustomChecksUserControl(ClangTidyCustomChecksOptionsView clangTidyCustomChecksOption)
    {
      InitializeComponent();

      wpfPropGrid.SelectedObject = clangTidyCustomChecksOption;

      Loaded += delegate
      {
        UIElementsActivator.Activate(HwndSource.FromVisual(this) as HwndSource);
      };

    }
  }
}
