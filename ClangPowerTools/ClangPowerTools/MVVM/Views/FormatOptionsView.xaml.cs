using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for FormatFileCreationView.xaml
  /// </summary>
  public partial class FormatOptionsView : Window
  {
    public FormatOptionsView()
    {
      InitializeComponent();
      DataContext = new FormatOptionsViewModel();
    }
  }
}
