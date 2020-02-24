using System.Windows;
using System.Windows.Controls;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for FormatFileCreationView.xaml
  /// </summary>
  public partial class FormatOptionsView : Window
  {
    private readonly FormatStyleViewModel formatStyleViewModel;

    public FormatOptionsView()
    {
      InitializeComponent();
      formatStyleViewModel = new FormatStyleViewModel(this);
      DataContext = formatStyleViewModel;
      CodeEditor.Text = "// --- Clang Power Tools - Format Style Editor ---\r\n//\r\n// Add your code here\r\n//\r\n// Format is run automatically on your code after \r\n// enabling any of the style options\r\n//\r\n// Check the Output tab to see your formatted code";
      CodeEditorReadOnly.Text = "// Your formatted code will be displayed here";
    
    }

    private void RunFormat_TextChanged(object sender, TextChangedEventArgs e)
    {
      formatStyleViewModel.RunFormat();
    }

    private void RunFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      formatStyleViewModel.RunFormat();
    }
  }
}
