using ClangPowerTools.MVVM.Models;
using ClangPowerToolsShared.MVVM.Commands;
using ClangPowerToolsShared.MVVM.ViewModels;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
      //var test = VsBrushes.WindowTextKey;
      var colorValues = VsColors.GetCurrentThemedColorValues();
      var cvPair = colorValues.Where(a => a.Key.ToString() == "ToolWindowTextColorKey").FirstOrDefault();
      tidyToolWindowViewModel = new TidyToolWindowViewModel(this);
      DataContext = tidyToolWindowViewModel;
      InitializeComponent();
    }

    public void UpdateView(List<string> filesPath)
    {
      tidyToolWindowViewModel.UpdateViewModel(filesPath);
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
