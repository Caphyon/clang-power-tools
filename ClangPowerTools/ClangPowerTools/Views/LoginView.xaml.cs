using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class UserControl1 : Window
  {
    public UserControl1()
    {
      InitializeComponent();
    }

    private void ForgotPasswordButton_Click(object sender, RoutedEventArgs e)
    {
      Process.Start(new ProcessStartInfo("https://api.clangpowertools.com/api/5d011c6a375f6b5ed9716629/user/forgot-password"));
    }

    private void SingUpButton_Click(object sender, RoutedEventArgs e)
    {
      Process.Start(new ProcessStartInfo("https://api.clangpowertools.com/api/5d011c6a375f6b5ed9716629/user/register"));
    }
  }
}
