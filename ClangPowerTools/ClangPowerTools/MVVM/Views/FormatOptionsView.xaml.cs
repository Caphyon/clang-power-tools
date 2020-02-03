using System.Windows;
using System.Windows.Controls;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for FormatFileCreationView.xaml
  /// </summary>
  public partial class FormatOptionsView : Window
  {
    private FormatStyleOptionsViewModel formatStyleOptionsViewModel;


    public FormatOptionsView()
    {
      InitializeComponent();
      formatStyleOptionsViewModel = new FormatStyleOptionsViewModel(this);
      DataContext = formatStyleOptionsViewModel;
      Owner = SettingsProvider.SettingsView;
    }

    private void CodeEditor_TextChanged(object sender, TextChangedEventArgs e)
    {
      formatStyleOptionsViewModel.HighlightText();
    }
  }
}
