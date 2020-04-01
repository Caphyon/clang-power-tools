using System.Windows;
using System.Windows.Controls;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for FormatFileCreationView.xaml
  /// </summary>
  public partial class FormatEditorView : Window
  {
    private readonly FormatEditorViewModel formatEditorViewModel;

    private const string inputWindowDefaulText = "// --- Clang Power Tools - Format Style Editor ---\r\n//\r\n// Add your code here\r\n//\r\n// Format is run automatically \r\n//\r\n// Check the OUTPUT tab to see your formatted code";
    private const string putputWindowDefaulText = "// Your formatted code will be displayed here";

    public FormatEditorView()
    {
      InitializeComponent();
      formatEditorViewModel = new FormatEditorViewModel(this);
      DataContext = formatEditorViewModel;
      CodeEditor.Text = inputWindowDefaulText;
      CodeEditorReadOnly.Text = putputWindowDefaulText;
    }

    private void RunFormat_TextChanged(object sender, TextChangedEventArgs e)
    {
      object interactable = (e.OriginalSource as FrameworkElement).DataContext;
      ChangeSelectedItem(interactable);
      formatEditorViewModel.RunFormat();
    }

    private void RunFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      object interactable = (e.OriginalSource as FrameworkElement).DataContext;
      ChangeSelectedItem(interactable);
      formatEditorViewModel.RunFormat();
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

    private void CodeEditor_PreviewDragOver(object sender, DragEventArgs e)
    {
      formatEditorViewModel.PreviewDragOver(e);
    }

    private void CodeEditor_PreviewDrop(object sender, DragEventArgs e)
    {
      formatEditorViewModel.PreviewDrop(e);
    }

  }
}
