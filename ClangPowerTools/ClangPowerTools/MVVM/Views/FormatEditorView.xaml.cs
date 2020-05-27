using ClangPowerTools.MVVM.Models;
using System;
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
    private const string outputWindowDefaulText = "// Your formatted code will be displayed here";

    public FormatEditorView()
    {
      InitializeComponent();
      formatEditorViewModel = new FormatEditorViewModel(this);
      DataContext = formatEditorViewModel;
      CodeEditor.Text = inputWindowDefaulText;
      CodeEditorReadOnly.Text = outputWindowDefaulText;
    }

    private void RunFormat_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (EnableSelectedTidyCheck(sender) == false) return;
      formatEditorViewModel.RunFormat();
    }

    private void RunFormat_Editor(object sender, EventArgs e)
    {
      formatEditorViewModel.RunFormat();
    }

    private void BooleanCombobox_DropDownClosed(object sender, EventArgs e)
    {
      if (EnableSelectedTidyCheck(sender) == false) return;
      formatEditorViewModel.RunFormat();
    }

    private void EnableTidyCheck(object sender, RoutedEventArgs e)
    {
      if (EnableSelectedTidyCheck(sender) == false) return;
    }

    private void OpenMultipleInput(object sender, RoutedEventArgs e)
    {
      var element = (sender as FrameworkElement).DataContext;
      if (element == null) return;
      formatEditorViewModel.OpenMultipleInput(FormatOptions.Items.IndexOf(element));      
    }

    private bool EnableSelectedTidyCheck(object sender)
    {
      if (!(sender is ListViewItem tidyCheck)) return false;
      
      tidyCheck.IsSelected = true;      
      return true;
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
