using ClangPowerTools.MVVM.Models;
using System.Collections.Generic;
using System.Windows;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for InputFormatStylesView.xaml
  /// </summary>
  public partial class ToggleMultipleDataView : Window
  {
    public ToggleMultipleDataView(List<ToggleModel> input)
    {
      InitializeComponent();
      DataContext = new ToggleMultipleDataViewModel(input);
      Owner = SettingsProvider.FormatEditorView;
    }
  }
}
