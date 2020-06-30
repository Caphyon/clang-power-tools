using System.Windows.Controls;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for AccountView.xaml
  /// </summary>
  public partial class AccountView : UserControl
  {
    public AccountView()
    {
      InitializeComponent();
      DataContext = new AccountViewModel();
    }
  }
}
