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

    private void DownloadButton(object sender, RoutedEventArgs e)
    {
      var elementIndex = GetElementIndex(sender as FrameworkElement);
      dataContext.DownloadCommand(elementIndex);
    }

    private void CancelButton(object sender, RoutedEventArgs e)
    {
      dataContext.CancelCommand();
    }

    private void UninstallButton(object sender, RoutedEventArgs e)
    {
      var elementIndex = GetElementIndex(sender as FrameworkElement);
      dataContext.UninstallCommand(elementIndex);
    }

    private int GetElementIndex(FrameworkElement frameworkElement)
    {
      var element = frameworkElement.DataContext;
      return VersionsList.Items.IndexOf(element);
    }
  }
}
