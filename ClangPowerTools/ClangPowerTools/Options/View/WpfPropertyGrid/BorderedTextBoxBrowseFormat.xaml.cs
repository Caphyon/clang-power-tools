using System;
using System.Windows;
using System.Windows.Documents;

namespace ClangPowerTools.Options.View.WpfPropertyGrid
{
  /// <summary>
  /// Interaction logic for BorderedTextBoxBrowse.xaml
  /// </summary>
  public partial class BorderedTextBoxBrowseFormat : TextBoxNotifaiableUserControl
  {
    public BorderedTextBoxBrowseFormat()
    {
      InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      // Create OpenFileDialog
      Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

      // Set filter for file extension and default file extension
      dlg.DefaultExt = ".exe";
      dlg.Filter = "clang-format|clang-format.exe|Executable files|*.exe|All files|*.*";

      // Display OpenFileDialog by calling ShowDialog method
      bool? result = dlg.ShowDialog();

      // Get the selected file name and display in a TextBox
      if (result == true)
      {
        // Open document
        string filename = dlg.FileName;
        TextEditor.Text = filename;

        Paragraph paragraph = new Paragraph();
        paragraph.Inlines.Add(System.IO.File.ReadAllText(filename));
      }
    }
  }
}
