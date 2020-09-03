using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for DetectFormatStyleMenuView.xaml
  /// </summary>
  public partial class DetectFormatStyleMenuView : Window
  {
    public DetectFormatStyleMenuView(FormatEditorViewModel formatEditorViewModel)
    {
      InitializeComponent();
      DataContext = new DetectFormatStyleMenuViewModel(this, formatEditorViewModel);
    }
  }
}