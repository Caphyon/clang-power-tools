using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for DetectFormatStyleMenuView.xaml
  /// </summary>
  public partial class DetectStyleFileSelectorView : Window
  {
    private readonly DetectStyleFileSelectorViewModel fileSelectorViewModel;

    public DetectStyleFileSelectorView()
    {
      InitializeComponent();
      fileSelectorViewModel = new DetectStyleFileSelectorViewModel(this);
      DataContext = fileSelectorViewModel;
    }
  }
}