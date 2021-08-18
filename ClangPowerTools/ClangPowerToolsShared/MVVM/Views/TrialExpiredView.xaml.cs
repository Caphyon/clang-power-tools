using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for License.xaml
  /// </summary>
  public partial class TrialExpiredView : Window
  {
    public TrialExpiredView()
    {
      InitializeComponent();
      DataContext = new TrialExpiredViewModel(this);
    }

  }
}
