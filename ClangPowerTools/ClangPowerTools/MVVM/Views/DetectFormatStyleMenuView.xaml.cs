using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for DetectFormatStyleMenuView.xaml
  /// </summary>
  public partial class DetectFormatStyleMenuView : Window
  {

    private readonly InputDataViewModel inputDataViewModel;

    public DetectFormatStyleMenuView()
    {
      InitializeComponent();
      inputDataViewModel = new InputDataViewModel(this, true);
      DataContext = inputDataViewModel;
    }
  }
}