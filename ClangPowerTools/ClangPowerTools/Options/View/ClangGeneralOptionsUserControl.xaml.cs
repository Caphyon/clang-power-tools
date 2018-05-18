using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace ClangPowerTools.Options.View
{
  /// <summary>
  /// Interaction logic for ClangGeneralOptionsUserControl.xaml
  /// </summary>
  public partial class ClangGeneralOptionsUserControl : UserControl
  {
    public ClangGeneralOptionsUserControl(ClangGeneralOptionsView clangGeneralOptions)
    {
      InitializeComponent();

      wpfPropGrid.SelectedObject = clangGeneralOptions;

      Loaded += delegate
      {
        UIElementsActivator.Activate(HwndSource.FromVisual(this) as HwndSource);
      };
    }

  }
}


// ShowGroupingOption
