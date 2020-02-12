using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for FormatFileCreationView.xaml
  /// </summary>
  public partial class FormatOptionsView : Window
  {
    private readonly FormatStyleOptionsViewModel formatStyleOptionsViewModel;
    private bool isPasteCommand = false;

    public FormatOptionsView()
    {
      InitializeComponent();
      formatStyleOptionsViewModel = new FormatStyleOptionsViewModel(this);
      DataContext = formatStyleOptionsViewModel;
      Owner = SettingsProvider.SettingsView;
    }

    private void CodeEditor_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (isPasteCommand)
      {
        isPasteCommand = false;
        formatStyleOptionsViewModel.RestCodeEditorFormat();
        return;
      }
      //formatStyleOptionsViewModel.HighlightText();
    }

    private void CodeEditor_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
      {
        isPasteCommand = true;
      }
    }
  }
}
