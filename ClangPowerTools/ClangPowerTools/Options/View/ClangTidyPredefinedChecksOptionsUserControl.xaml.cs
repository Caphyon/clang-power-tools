using System.Windows.Controls;
using System.Windows.Interop;

namespace ClangPowerTools.Options.View
{
  /// <summary>
  /// Interaction logic for ClangTidyChecksOptionsUserControl.xaml
  /// </summary>
  public partial class ClangTidyPredefinedChecksOptionsUserControl : UserControl
  {
    public ClangTidyPredefinedChecksOptionsUserControl(ClangTidyPredefinedChecksOptionsView clangTidyChecksOptions)
    {
      InitializeComponent();

      wpfPropGrid.SelectedObject = clangTidyChecksOptions;

      Loaded += delegate
      {
        UIElementsActivator.Activate(HwndSource.FromVisual(this) as HwndSource);
      };

    }

    
    public void CleanQuickSearch()
    {
      wpfPropGrid.SearchText = string.Empty;
      wpfPropGrid.PropertyList.Focus();
    }

    private void WpfPropGrid_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {

    }
  }
}
