using System.Windows;
using System.Windows.Controls;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for FormatFileCreationView.xaml
  /// </summary>
  public partial class FormatEditorView : Window
  {
    private readonly FormatEditorViewModel formatStyleViewModel;

    private const string inputWindowDefaulText = "// --- Clang Power Tools - Format Style Editor ---\r\n//\r\n// Add your code here\r\n//\r\n// Format is run automatically on your code after \r\n// enabling any of the style options\r\n//\r\n// Check the Output tab to see your formatted code";
    private const string putputWindowDefaulText = "// Your formatted code will be displayed here";

    public FormatEditorView()
    {
      InitializeComponent();
      formatStyleViewModel = new FormatEditorViewModel(this);
      DataContext = formatStyleViewModel;
      CodeEditor.Text = inputWindowDefaulText;
      CodeEditorReadOnly.Text = putputWindowDefaulText;
    }

    private void RunFormat_TextChanged(object sender, TextChangedEventArgs e)
    {
      object interactable = (e.OriginalSource as FrameworkElement).DataContext;
      ChangeSelectedItem(interactable);
      formatStyleViewModel.RunFormat();
    }

    private void RunFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      object interactable = (e.OriginalSource as FrameworkElement).DataContext;
      ChangeSelectedItem(interactable);
      formatStyleViewModel.RunFormat();
    }

    private void ModifyFocus(object sender, RoutedEventArgs e)
    {
      object interactable = (e.OriginalSource as FrameworkElement).DataContext;
      ChangeSelectedItem(interactable);
    }

    private void ChangeSelectedItem(object interactable)
    {
      if (!(FormatOptions.ItemContainerGenerator.ContainerFromItem(interactable) is ListViewItem selectedItem)) return;
      selectedItem.IsSelected = true;
    }
  }
}
