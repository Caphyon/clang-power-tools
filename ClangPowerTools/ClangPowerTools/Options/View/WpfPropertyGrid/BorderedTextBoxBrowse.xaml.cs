using System;
using System.Windows;
using System.Windows.Documents;

namespace ClangPowerTools.Options.View.WpfPropertyGrid
{
  /// <summary>
  /// Interaction logic for BorderedTextBoxBrowse.xaml
  /// </summary>
  public partial class BorderedTextBoxBrowse : TextBoxNotifaiableUserControl
  {
    public BorderedTextBoxBrowse()
    {
      InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      // Create OpenFileDialog
      Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

      // Set filter for file extension and default file extension
      //dlg.FileName = "clang-format.exe";

      dlg.DefaultExt = ".exe";
      dlg.Filter = "clang-format|clang-format.exe|Executable files|*.exe|All files|*.*";

      //dlg.FilterIndex = 1;

      // Display OpenFileDialog by calling ShowDialog method
      Nullable<bool> result = dlg.ShowDialog();

      // Get the selected file name and display in a TextBox
      if (result == true)
      {
        // Open document
        string filename = dlg.FileName;
        TextEditor.Text = filename;

        Paragraph paragraph = new Paragraph();
        paragraph.Inlines.Add(System.IO.File.ReadAllText(filename));
        //FlowDocument document = new FlowDocument(paragraph);
        //FlowDocReader.Document = document;
      }
    }
  }
}
