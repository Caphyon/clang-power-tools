using System.Windows;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for InputFormatStylesView.xaml
  /// </summary>
  public partial class InputFormatStylesView : Window
  {
    public InputFormatStylesView()
    {
      InitializeComponent();
      DataContext = new InputFormatStylesView();
    }
  }
}
