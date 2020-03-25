using System.Windows;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for InputFormatStylesView.xaml
  /// </summary>
  public partial class InputMultipleDataView : Window
  {
    public InputMultipleDataView()
    {
      InitializeComponent();
      DataContext = new InputMultipleDataViewModel();
    }
  }
}
