using System.Diagnostics;
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

    private void Hyperlink_SupportGmail(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      Process.Start(new ProcessStartInfo("mailto:support@clangpowertools.com"));
      e.Handled = true;
    }
  }
}
