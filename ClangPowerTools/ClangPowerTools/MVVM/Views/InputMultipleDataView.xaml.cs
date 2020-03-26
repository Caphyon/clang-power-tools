using System.Windows;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for InputFormatStylesView.xaml
  /// </summary>
  public partial class InputMultipleDataView : Window
  {
    public InputMultipleDataView(string input)
    {
      InitializeComponent();
      DataContext = new InputMultipleDataViewModel(input); ;
      Owner = SettingsProvider.FormatEditorView;
    }
  }
}
