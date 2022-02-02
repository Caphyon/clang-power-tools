using ClangPowerTools.MVVM.Models;
using ClangPowerToolsShared.MVVM.Commands;
using ClangPowerToolsShared.MVVM.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

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
      //List<User> items = new List<User>();
      //items.Add(new User() { Name = "John Doe", Age = 42, Sex = SexType.Male });
      //items.Add(new User() { Name = "Jane Doe", Age = 39, Sex = SexType.Female });
      //items.Add(new User() { Name = "Sammy Doe", Age = 13, Sex = SexType.Male });
      //lvUsers.ItemsSource = items;

      //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvUsers.ItemsSource);
      //PropertyGroupDescription groupDescription = new PropertyGroupDescription("Sex");
      //view.GroupDescriptions.Add(groupDescription);
    }

    public void UpdateView(List<string> filesPath)
    {
      tidyToolWindowViewModel.UpdateViewModel(filesPath);
    }
    public enum SexType { Male, Female };

    public class User
    {
      public string Name { get; set; }

      public int Age { get; set; }

      public string Mail { get; set; }

      public SexType Sex { get; set; }
    }
    public void OpenTidyToolWindow(List<string> filesPath)
    {
      tidyToolWindowViewModel.OpenTidyToolWindow(filesPath);
    }


    private void DiffButton(object sender, RoutedEventArgs e)
    {
      var elementIndex = sender as FrameworkElement;
      var element = elementIndex.DataContext as FileModel;
      if (element != null)
      {
        tidyToolWindowViewModel.DiffFile(element);
      }
    }

    private void FixButton(object sender, RoutedEventArgs e)
    {
      var elementIndex = sender as FrameworkElement;
      var element = elementIndex.DataContext as FileModel;
      if (element != null)
      {
        tidyToolWindowViewModel.FixAllFilesAsync(element).SafeFireAndForget();
      }
    }

    private void CheckAll(object sender, RoutedEventArgs e)
    {
      tidyToolWindowViewModel.CheckOrUncheckAll();
    }
    private void CheckBox_Click(object sender, RoutedEventArgs e)
    {
      var elementIndex = sender as FrameworkElement;
      var element = elementIndex.DataContext as FileModel;
      if (element != null)
      {
        tidyToolWindowViewModel.UpdateCheckedNumber(element);
      }
    }

    private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      var item = sender as ListViewItem;
      var file = item.Content as FileModel;
      if (item != null && item.IsSelected)
      {
        FileCommand.TidyFixDiff(file);
      }
    }
  }
}
