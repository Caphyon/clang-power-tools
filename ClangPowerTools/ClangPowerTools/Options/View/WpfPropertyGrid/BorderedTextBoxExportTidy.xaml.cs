using System;
using System.Windows;
using System.Windows.Documents;

namespace ClangPowerTools.Options.View.WpfPropertyGrid
{
  /// <summary>
  /// Interaction logic for BorderedTextBoxExportTidy.xaml
  /// </summary>
  public partial class BorderedTextBoxExportTidy : TextBoxNotifaiableUserControl
  {
    public BorderedTextBoxExportTidy()
    {
      InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      // Create dialog window
      Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

      // Set the default file extension
      dlg.DefaultExt = ".clang-tidy";
      dlg.Filter = "clang-tidy";

      //Display the dialog window
      bool? result = dlg.ShowDialog();

      if(result == true)
      {
        string filename = dlg.FileName;
      }
    }
  }
}
