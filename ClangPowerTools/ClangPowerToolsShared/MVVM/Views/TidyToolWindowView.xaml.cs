using ClangPowerTools.Commands;
using ClangPowerTools.MVVM.Models;
using ClangPowerToolsShared.MVVM.Commands;
using ClangPowerToolsShared.MVVM.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
      tidyToolWindowViewModel = new TidyToolWindowViewModel(this);
      DataContext = tidyToolWindowViewModel;
      InitializeComponent();
    }

    public void UpdateView(List<string> filesPath)
    {
      tidyToolWindowViewModel.UpdateViewModel(filesPath);
    }

    private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      var item = sender as ListViewItem;
      var i = item.Content as FileModel;
      if (item != null && item.IsSelected)
      {
        TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyFixId, CommandUILocation.ContextMenu, null, new List<string> { i.FullFileName});
        //tidyDiffCommand.TidyDiff(i.FullFileName);
        string tempFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ClangPowerTools", "Temp", i.FileName);
        TidyDiffCommand.DiffFilesUsingDefaultTool(tempFolderPath, i.FullFileName);  
      }
    }
  }
}
