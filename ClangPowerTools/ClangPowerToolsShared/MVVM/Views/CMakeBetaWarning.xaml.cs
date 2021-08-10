using ClangPowerTools.Helpers;
using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for CMakeBetaWarning.xaml
  /// </summary>
  public partial class CMakeBetaWarning : Window
  {
    private readonly string registryName = @"Software\Caphyon\Clang Power Tools";
    private readonly string keyName = "CMakeBetaWarning";

    public CMakeBetaWarning()
    {
      InitializeComponent();
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void CheckBox_Checked(object sender, RoutedEventArgs e)
    {
      var registryUtility = new RegistryUtility(registryName);
      registryUtility.WriteCurrentUserKey(keyName, "DoNotShow");
    }

    private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
      var registryUtility = new RegistryUtility(registryName);
      registryUtility.DeleteCurrentUserKey(keyName);
    }
  }
}
