using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.MVVM.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
      findToolWindowViewModel = new FindToolWindowViewModel(this);
      DataContext = findToolWindowViewModel;
      InitializeComponent();
    }

    public void OpenFindToolWindow(List<string> filesPath)
    {
      findToolWindowViewModel.OpenToolWindow(filesPath);
    }

    public void RunQuery()
    {
      findToolWindowViewModel.RunQuery();
    }

    private void Matcher_Click(object sender, RoutedEventArgs e)
    {
      var item = (sender as ListView).SelectedItem;
      if (item != null)
      {
        var type = item.GetType();
        if (type.IsGenericType)
        {
          if (type == typeof(KeyValuePair<int,string>))
          {
            var key = type.GetProperty("Key");
            var value = type.GetProperty("Value");
            var keyObj = key.GetValue(item, null);
            var valueObj = value.GetValue(item, null);
            var keyValueResult =  new KeyValuePair<object, object>(keyObj, valueObj);
            findToolWindowViewModel.SelectCommandToRun((int)keyValueResult.Key);
          }
        }
      }
    }
  }
}
