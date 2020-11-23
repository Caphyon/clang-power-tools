using ClangPowerTools.MVVM.Interfaces;
using System.Windows;
using System.Windows.Controls;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for LlvmSettingsView.xaml
  /// </summary>
  public partial class LlvmSettingsView : UserControl, IView
  {
    private LlvmSettingsViewModel llvmSettingsViewModel;

    public LlvmSettingsView()
    {
      llvmSettingsViewModel = new LlvmSettingsViewModel(this);
      DataContext = llvmSettingsViewModel;
      InitializeComponent();
      this.Loaded += LlvmSettingsViewLoaded;
      SettingsHandler.RefreshSettingsView += ResetView;
    }

    public void ResetView()
    {
      llvmSettingsViewModel = new LlvmSettingsViewModel(this);
      DataContext = llvmSettingsViewModel;
    }

    private void DownloadButton(object sender, RoutedEventArgs e)
    {
      var elementIndex = GetElementIndex(sender as FrameworkElement);
      llvmSettingsViewModel.DownloadCommand(elementIndex);
    }

    private void CancelButton(object sender, RoutedEventArgs e)
    {
      llvmSettingsViewModel.CancelCommand();
    }

    private void UninstallButton(object sender, RoutedEventArgs e)
    {
      var elementIndex = GetElementIndex(sender as FrameworkElement);
      llvmSettingsViewModel.UninstallCommand(elementIndex);
    }

    private int GetElementIndex(FrameworkElement frameworkElement)
    {
      var element = frameworkElement.DataContext;
      return VersionsList.Items.IndexOf(element);
    }

    private void LlvmSettingsViewLoaded(object sender, RoutedEventArgs e)
    {
      Window window = Window.GetWindow(this);
      window.Closing += llvmSettingsViewModel.WindowClosed;
    }
  }
}
