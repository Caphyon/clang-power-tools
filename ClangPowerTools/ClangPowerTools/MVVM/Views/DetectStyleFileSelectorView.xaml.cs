using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for DetectFormatStyleMenuView.xaml
  /// </summary>
  public partial class DetectStyleFileSelectorView : Window
  {

    private readonly InputDataViewModel inputDataViewModel;

    public DetectStyleFileSelectorView()
    {
      InitializeComponent();
      inputDataViewModel = new InputDataViewModel(this, true);
      DataContext = inputDataViewModel;
    }
  }
}