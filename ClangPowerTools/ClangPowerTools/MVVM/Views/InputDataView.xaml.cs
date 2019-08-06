using System.Windows;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for InputDataView.xaml
  /// </summary>
  public partial class InputDataView : Window
  {
    public InputDataView(InputDataViewModel inputDataViewModel)
    {
      InitializeComponent();
      DataContext = inputDataViewModel;
    }
  }
}
