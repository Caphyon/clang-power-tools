using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClangPowerTools.Options.View
{
  /// <summary>
  /// Interaction logic for ButtonTidyExportControl.xaml
  /// </summary>
  public partial class ButtonTidyExportControl : UserControl
  {
    public ButtonTidyExportControl()
    {
      InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      // Create dialog window
      Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

      // Set the default file extension
      dlg.DefaultExt = ".clang-tidy";
      dlg.Filter = "Configuration files (.clang-tidy)|*.clang-tidy";

      //Display the dialog window
      bool? result = dlg.ShowDialog();

      if (result == true)
      {
        string filename = dlg.FileName;
      }
    }
  }
}
