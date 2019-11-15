using System.Windows;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for InputDataView.xaml
  /// </summary>
  public partial class InputDataView : Window
  {
    private InputDataViewModel inputDataViewModel = new InputDataViewModel();
    public InputDataView(InputDataViewModel inputDataViewModel)
    {
      InitializeComponent();
      //TODO might remove inputDataViewModel paramater from constructor
      this.inputDataViewModel = inputDataViewModel;
      DataContext = inputDataViewModel;
      Owner = SettingsProvider.SettingsView;
    }

    private void DeleteButton(object sender, RoutedEventArgs e)
    {
      var elementIndex = GetElementIndex(sender as FrameworkElement);
      inputDataViewModel.DeleteInput(elementIndex);
    }

    private int GetElementIndex(FrameworkElement frameworkElement)
    {
      var element = frameworkElement.DataContext;
      return InputsList.Items.IndexOf(element);
    }
  }
}
