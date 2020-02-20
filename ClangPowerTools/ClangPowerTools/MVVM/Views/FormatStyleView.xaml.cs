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
      CodeEditor.Text = "// Add your code here";
    }
  }
}
