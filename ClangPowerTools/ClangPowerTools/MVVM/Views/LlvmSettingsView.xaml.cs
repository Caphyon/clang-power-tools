using System.Windows;
using System.Windows.Controls;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for LlvmSettingsView.xaml
  /// </summary>
  public partial class LlvmSettingsView : UserControl
  {
    private readonly LlvmSettingsViewModel dataContext = new LlvmSettingsViewModel();

    public LlvmSettingsView()
    {
      InitializeComponent();
      DataContext = dataContext;
    }

    private void SetIndex(object sender, RoutedEventArgs e)
    {
      var element = (sender as FrameworkElement).DataContext;
      var elementIndex = VersionsList.Items.IndexOf(element);
      dataContext.SetSelectedElement(elementIndex);
    }
  }
}
