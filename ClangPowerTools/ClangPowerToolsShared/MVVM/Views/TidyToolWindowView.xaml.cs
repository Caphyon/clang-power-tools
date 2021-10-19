﻿using ClangPowerTools.Commands;
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

    private void DiscardButton(object sender, RoutedEventArgs e)
    {
      var elementIndex = sender as FrameworkElement;
      var element = elementIndex.DataContext as FileModel;
      if (element != null)
      {
        tidyToolWindowViewModel.DiscardFile(element);
      }
    }

    private void DiffButton(object sender, RoutedEventArgs e)
    {
      var elementIndex = sender as FrameworkElement;
      var element = elementIndex.DataContext as FileModel;
      if (element != null)
      {
        FileCommand.DiffFilesUsingDefaultTool(element.CopyFullFileName, element.FullFileName);
      }
    }

    private void FixButton(object sender, RoutedEventArgs e)
    {
      var elementIndex = sender as FrameworkElement;
      var element = elementIndex.DataContext as FileModel;
      if (element != null)
      {
        FileCommand.CopyFileInTemp(element);
        CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kTidyFixId, CommandUILocation.ContextMenu, new List<string> { element.FullFileName });
        FileCommand.DiffFilesUsingDefaultTool(element.CopyFullFileName, element.FullFileName);
        tidyToolWindowViewModel.MarkFixedFile(element);
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
