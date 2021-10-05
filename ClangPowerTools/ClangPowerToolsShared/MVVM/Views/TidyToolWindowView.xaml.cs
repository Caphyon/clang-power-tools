using ClangPowerToolsShared.MVVM.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for CompilerSettingsView.xaml
  /// </summary>
  public partial class TidyToolWindowView : UserControl
  {
    public TidyToolWindowView()
    {
      DataContext = new TidyToolWindowViewModel(this);

      InitializeComponent();
    }


    private void button1_Click(object sender, RoutedEventArgs e)
    {
      MessageBox.Show(
          string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
          "TidyToolWindow");
    }
  }
}
