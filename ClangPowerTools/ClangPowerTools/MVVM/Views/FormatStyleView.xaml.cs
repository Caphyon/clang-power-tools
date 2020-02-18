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
    private readonly FormatStyleViewModel formatStyleViewModel;
    private bool isPasteCommand = false;

    public FormatOptionsView()
    {
      InitializeComponent();
      formatStyleViewModel = new FormatStyleViewModel(this);
      DataContext = formatStyleViewModel;
      TextManipulation.ReplaceAllTextInFlowDocument(CodeEditor.Document, "//Add your code here");
    }

    private void CodeEditor_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (isPasteCommand)
      {
        isPasteCommand = false;
        formatStyleViewModel.RestCodeEditorFormat();
        return;
      }
      formatStyleViewModel.HighlightText();
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
