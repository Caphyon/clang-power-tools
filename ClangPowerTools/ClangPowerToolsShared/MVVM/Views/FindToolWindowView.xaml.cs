using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.MVVM;
using ClangPowerToolsShared.MVVM.AutoCompleteHistory;
using ClangPowerToolsShared.MVVM.Interfaces;
using ClangPowerToolsShared.MVVM.Models.ToolWindowModels;
using ClangPowerToolsShared.MVVM.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Process = System.Diagnostics.Process;

namespace ClangPowerTools.Views
{
  /// <summary>
  /// Interaction logic for CompilerSettingsView.xaml
  /// </summary>
  public partial class FindToolWindowView : UserControl
  {
    private FindToolWindowViewModel findToolWindowViewModel;

    public FindToolWindowView()
    {
      var findToolWindowHandler = new FindToolWindowHandler();
      findToolWindowHandler.LoadFindToolWindowData();
      findToolWindowViewModel = new FindToolWindowViewModel(this);
      DataContext = findToolWindowViewModel;
      InitializeComponent();
    }

    public void OpenFindToolWindow() { }

    public void RunQuery()
    {
      findToolWindowViewModel.RunQuery();
    }

    private void MatchDefaultArgs_click(object sender, RoutedEventArgs e)
    {
      findToolWindowViewModel.RunCommandFromView();
    }

    private void Hyperlink_Feedback(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      Process.Start(new ProcessStartInfo("https://clangpowertools.com/contact.html"));
      e.Handled = true;
    }

    private void ComboBox_Loaded(object sender, RoutedEventArgs e)
    {
      var combo = sender as ComboBox;
      combo.ItemsSource = LookInMenuController.MenuOptions;
      combo.SelectedIndex = 0;
    }

    private void OnKeyDownHandler(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        if (!string.IsNullOrEmpty(Matches.Text))
          findToolWindowViewModel.RunCommandFromView();
      }
    }

    private void Pin_click(object sender, RoutedEventArgs e)
    {
      var elementIndex = sender as FrameworkElement;
      var element = elementIndex.DataContext as AutoCompleteHistoryModel;
      if (element != null)
      {
        if (element.Pin())
          findToolWindowViewModel.AddPinOnRightPlace(element);
      }
    }

    private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ListView listView = e.Source as ListView;
      if (listView.ItemContainerGenerator?.ContainerFromItem(listView.SelectedItem)
              is FrameworkElement container)
      {

        Matches.TextChanged -= AutoCompleteBehavior.onTextChanged;
        var item = container?.DataContext as AutoCompleteHistoryModel;
        Matches.Text = AutoCompleteBehavior.MatchText + item?.Value;
        if (AutoCompleteBehavior.MatchText is null)
          AutoCompleteBehavior.MatchText = string.Empty;
        Matches.CaretIndex = AutoCompleteBehavior.MatchText.Length;
        Matches.SelectionStart = AutoCompleteBehavior.MatchText.Length;
        Matches.SelectionLength = item.Value.Length;
        Matches.TextChanged += AutoCompleteBehavior.onTextChanged;
      }
    }

    private void ComboBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
      if (menu.SelectedItem != null)
      {
        var item = menu.SelectedItem as ClangPowerToolsShared.Commands.MenuItem;
        LookInMenuController.SetSelectedOption(item);
        PowerShellWrapper.EndIneractiveMode();
      }
    }

    private void Matcher_Click(object sender, RoutedEventArgs e)
    {
      var item = (sender as ListView).SelectedItem as IViewMatcher;
      if (item != null)
      {
        findToolWindowViewModel.SelectCommandToRun(item);
      }
    }
  }
}
