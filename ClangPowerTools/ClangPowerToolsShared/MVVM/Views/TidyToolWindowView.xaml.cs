using ClangPowerTools.MVVM.Interfaces;
using ClangPowerToolsShared.MVVM.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for CompilerSettingsView.xaml
  /// </summary>
  public partial class TidyToolWindowView : UserControl
  {
    public  TidyToolWindowView()
    {
      DataContext = new TidyToolWindowViewModel(this);
      InitializeComponent();
      //List<User> items = new List<User>();
      //items.Add(new User() { Name = "John Doe", Age = 42, Mail = "john@doe-family.com" });
      //items.Add(new User() { Name = "Jane Doe", Age = 39, Mail = "jane@doe-family.com" });
      //items.Add(new User() { Name = "Sammy Doe", Age = 7, Mail = "sammy.doe@gmail.com" });
      //lvUsers.ItemsSource = items;
    }

    public class User
    {
      public string Name { get; set; }

      public int Age { get; set; }

      public string Mail { get; set; }
    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
      MessageBox.Show(
          string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
          "TidyToolWindow");
    }
  }
}
