using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for DetectFormatStyleMenuView.xaml
  /// </summary>
  public partial class DetectFormatStyleMenuView : Window
  {

    private readonly InputDataViewModel inputDataViewModel = new InputDataViewModel(true);

    public DetectFormatStyleMenuView()
    {
      InitializeComponent();
      DataContext = inputDataViewModel;
    }
  }
}