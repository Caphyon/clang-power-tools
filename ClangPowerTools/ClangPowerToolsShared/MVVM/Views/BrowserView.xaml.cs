using System;
using System.Windows;
using System.Windows.Navigation;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for Browser.xaml
  /// </summary>
  public partial class BrowserView : Window
  {
    private readonly BrowserViewModel viewModel;
    private readonly string tidyCheckName;

    public BrowserView(string tidyCheckName)
    {
      InitializeComponent();
      this.tidyCheckName = tidyCheckName;
      viewModel = new BrowserViewModel();
      DataContext = viewModel;
    }

    public void OpenDescription()
    {
      Uri uri = viewModel.CreateFlagUri(tidyCheckName);
      webBrowser.Navigated += WebBrowser_LoadCompleted;
      webBrowser.Navigate(uri);
    }

    private void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
    {
      webBrowser.Navigated -= WebBrowser_LoadCompleted;
      Show();
    }
  }
}
