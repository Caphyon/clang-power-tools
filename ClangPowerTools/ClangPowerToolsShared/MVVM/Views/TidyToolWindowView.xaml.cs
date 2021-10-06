using ClangPowerToolsShared.MVVM.ViewModels;
using System;
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
    private TidyToolWindowViewModel tidyToolWindowViewModel;
    public TidyToolWindowView()
    {
      tidyToolWindowViewModel = new TidyToolWindowViewModel(this);
      DataContext = tidyToolWindowViewModel;
      InitializeComponent();
    }

    public void UpdateView(List<string> filesPath)
    {
      tidyToolWindowViewModel.UpdateViewModel(filesPath);
    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
      MessageBox.Show(
          string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
          "TidyToolWindow");
    }


  }
}
