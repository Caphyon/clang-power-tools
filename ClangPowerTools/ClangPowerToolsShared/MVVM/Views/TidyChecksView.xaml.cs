using System;
using System.Linq;
using System.Text;
using System.Windows;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for TidyChecksView.xaml
  /// </summary>
  public partial class TidyChecksView : Window
  {
    private readonly TidyChecksViewModel viewModel;

    public TidyChecksView()
    {
      InitializeComponent();

      viewModel = new TidyChecksViewModel(this);
      DataContext = viewModel;
      Owner = SettingsProvider.SettingsView;
    }

    private void ToggleButton_Checked(object sender, RoutedEventArgs e)
    {
      viewModel.MultipleStateChange(true);
      viewModel.DeactivateDefaultsToggle();
    }

    private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
    {
      viewModel.MultipleStateChange(false);
      viewModel.DeactivateDefaultsToggle();
    }

    private void OpenDescription(object sender, RoutedEventArgs e)
    {
      var elementIndex = GetElementIndex(sender as FrameworkElement);
      var tidyCheckModel = viewModel.TidyChecksList.ElementAt(elementIndex);
      viewModel.OpenBrowser(tidyCheckModel.Name);
    }

    private int GetElementIndex(FrameworkElement frameworkElement)
    {
      var element = frameworkElement.DataContext;
      return TidyChecksListBox.Items.IndexOf(element);
    }
  }
}
