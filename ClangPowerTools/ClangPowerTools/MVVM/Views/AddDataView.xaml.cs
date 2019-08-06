using System.Windows;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for AddDataView.xaml
  /// </summary>
  public partial class AddDataView : Window
  {
    public AddDataView(AddDataViewModel addDataViewModel)
    {
      InitializeComponent();
      DataContext = addDataViewModel;
    }
  }
}
