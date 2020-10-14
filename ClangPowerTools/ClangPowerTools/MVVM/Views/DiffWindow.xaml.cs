using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for DiffWindow.xaml
  /// </summary>
  public partial class DiffWindow : DialogWindow
  {
    #region Members

    private readonly DiffViewModel diffViewModel;

    #endregion

    #region Constructor
    public DiffWindow()
    {
      InitializeComponent();
      diffViewModel = new DiffViewModel(this);
      DataContext = diffViewModel;
    }

    #endregion

    #region Public Methods

    public async Task ShowDiffAsync(List<string> filesPath, DialogWindow detectingWindowOwner)
    {
      await diffViewModel.DiffDocumentsAsync(filesPath, detectingWindowOwner);
    }

    #endregion

    #region Private Methods

    private void Diff_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      if (e.VerticalChange == 0 && e.HorizontalChange == 0) return;
      if (sender == DiffInput)
      {
        DiffOutput.ScrollToVerticalOffset(e.VerticalOffset);
        DiffOutput.ScrollToHorizontalOffset(e.HorizontalOffset);
      }
      else
      {
        DiffInput.ScrollToVerticalOffset(e.VerticalOffset);
        DiffInput.ScrollToHorizontalOffset(e.HorizontalOffset);
      }
    }

    private void OptionInputChanged(object sender, TextChangedEventArgs e)
    {
      var element = (sender as FrameworkElement).DataContext;
      if (element == null) return;
      diffViewModel.OptionChanged(FormatOptions.Items.IndexOf(element));
    }

    private void OptionDropDownClosed(object sender, EventArgs e)
    {
      var element = (sender as FrameworkElement).DataContext;
      if (element == null) return;
      diffViewModel.OptionChanged(FormatOptions.Items.IndexOf(element));
    }

    private void OpenMultipleInput(object sender, RoutedEventArgs e)
    {
      var element = (sender as FrameworkElement).DataContext;
      if (element == null) return;
      int index = FormatOptions.Items.IndexOf(element);
      diffViewModel.OpenMultipleInput(index);
    }

    private void ResetOption(object sender, RoutedEventArgs e)
    {
      var element = (sender as FrameworkElement).DataContext;
      if (element == null) return;
      int index = FormatOptions.Items.IndexOf(element);
      diffViewModel.ResetOption(index);
    }

    #endregion
  }
}
