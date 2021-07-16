using System.Windows.Controls;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for FeedbackView.xaml
  /// </summary>
  public partial class FeedbackView : UserControl
  {
    public FeedbackView()
    {
      InitializeComponent();
      DataContext = new FeedbackViewModel();
    }
  }
}
